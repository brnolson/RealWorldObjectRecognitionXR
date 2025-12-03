### Install Requirements
```
pip install -r requirements.txt
```

On separate terminals run
## Run Backend Server
```
uvicorn main:app --reload --port 8000
```
## Forward local website to global
```
ngrok http 8000
```