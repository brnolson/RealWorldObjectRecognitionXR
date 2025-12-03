import base64
import os

from fastapi import FastAPI, UploadFile, File
from fastapi.responses import JSONResponse
from openai import OpenAI
from dotenv import load_dotenv

# Load environment, set up client
load_dotenv()
client = OpenAI(api_key=os.environ.get("OPENAI_API_KEY"))

app = FastAPI()


@app.get("/health")
def health():
    return {"status": "ok"}


@app.post("/transcribe")
async def transcribe(audio: UploadFile = File(...)):
    audio_bytes = await audio.read()
    print("/transcribe got", len(audio_bytes), "bytes")

    try:
        audio_file = ("audio.wav", audio_bytes, "audio/wav")

        tx = client.audio.transcriptions.create(
            model="whisper-1",
            file=audio_file
        )

        text = tx.text or ""
        return JSONResponse({"text": text})
    except Exception as e:
        print("Transcribe error:", e)
        # Return a normal JSON object so Unity never crashes
        return JSONResponse({"text": f"Unable to transcribe: {e}"}, status_code=200)

# Simple echo endpoint (optional, not used by Unity but handy for testing)
@app.post("/")
async def echo_root(
    audio: UploadFile = File(None),
    image: UploadFile = File(None),
):
    audio_bytes = await audio.read() if audio is not None else b""
    image_bytes = await image.read() if image is not None else b""
    reply = f"Got {len(audio_bytes)} audio bytes and {len(image_bytes)} image bytes."
    print("ROOT /:", reply)
    return JSONResponse({"reply": reply})


@app.post("/llm")
async def assistant(
    audio: UploadFile = File(None),
    image: UploadFile = File(None),
):
    # Safely read files
    audio_bytes = await audio.read() if audio else b""
    image_bytes = await image.read() if image else b""

    print(f"/llm received audio={len(audio_bytes)} bytes, image={len(image_bytes)} bytes")

    # Always define these so we can use them in except
    transcript_text = ""
    img_data_url = None

    try:
        # 1) Speech to text (Whisper)
        if audio_bytes:
            audio_file = ("audio.wav", audio_bytes, "audio/wav")
            transcription = client.audio.transcriptions.create(
                model="whisper-1",
                file=audio_file,
            )
            transcript_text = transcription.text or ""
            print("Whisper transcript:", transcript_text)

        # 2) Image to data URL
        if image_bytes:
            # full data URL used for the model
            b64 = base64.b64encode(image_bytes).decode("utf-8")
            img_data_url = f"data:image/png;base64,{b64}"

            # log only a SMALL piece so logs are readable
            print("Image converted to base64 (first 64):", img_data_url[:64], "...")
        else:
            img_data_url = None

        # Debug: save received image
        print("Received image bytes:", len(image_bytes))
        with open("debug_recv.png", "wb") as f:
            f.write(image_bytes)
        print("Saved debug_recv.png")

        # 3) Build multimodal content for GPT
        content = []

        if transcript_text:
            content.append({
                "type": "text",
                "text": f"User said: {transcript_text}. Explain what you see and answer their question."
            })
        else:
            content.append({
                "type": "text",
                "text": "No speech detected. Describe the scene in the image."
            })

        if img_data_url:
            content.append({
                "type": "image_url",
                "image_url": {"url": img_data_url}
            })

        completion = client.chat.completions.create(
            model="gpt-4.1-mini",
            messages=[
                {
                    "role": "user",
                    "content": content,
                }
            ],
        )

        reply = completion.choices[0].message.content
        print("GPT reply:", reply)

        return JSONResponse(
            {
                "reply": reply,
                "transcript": transcript_text,
            }
        )

    except Exception as e:
        # Important: never let the exception propagate as 500.
        print("ERROR in /llm:", repr(e))
        safe_reply = f"Backend error: {e}"
        return JSONResponse(
            {
                "reply": safe_reply,
                "transcript": transcript_text,
            },
            status_code=200,  # still 200 so Unity sees it as success
        )
