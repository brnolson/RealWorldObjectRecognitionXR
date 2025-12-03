### 1 Install Dependencies
Create a Python virtual environment
```
python -m venv venv
```
Activate it
```
# Windows
venv\Scripts\activate

# macOS / Linux
source venv/bin/activate
```

Install required packages
```
pip install -r requirements.txt
```

### 2. Start backend locally
Run FastAPI server
```
uvicorn main:app --reload --port 8000
```

### 3. Expose local backend to Internet
## Install ngrok
https://ngrok.com/download/
Sign up for free and get your authtoken and set it
```
ngrok config add-authtoken YOUR_TOKEN_HERE
```
Start the tunnel
```
ngrok http 8000
```
ngrok will show a public HTTPS URL:
```
Forwarding    https://xxxxx.ngrok-free.app  ->  http://localhost:8000
```
In Assets/Scripts/LlmClient.cs replace the BASEURL constant with the HTTPS URL shown by ngrok.
