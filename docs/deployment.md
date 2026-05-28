# Deployment Overview

The project ships two deployable units: the **Python RAG service** and the
**ASP.NET Core 8 MVC** application. Only the Python service has Docker
artifacts checked in.

## Python RAG service — Docker

Files: [`rag-service/Dockerfile`](../rag-service/Dockerfile),
[`rag-service/docker-compose.yml`](../rag-service/docker-compose.yml).

### Build + run with docker compose

```bash
cd rag-service
docker compose up --build
```

The compose file:

* builds the image from the local `Dockerfile`
* maps host port `8000` → container port `8000`
* loads environment from `rag-service/.env`
* mounts two volumes so data survives container rebuilds:
  * `./chroma_db` → `/app/chroma_db` (vector database)
  * `./storage`  → `/app/storage`  (uploaded documents)
* restarts unless explicitly stopped

```yaml
services:
  rag-api:
    build: .
    container_name: rag-prn222
    ports:
      - "8000:8000"
    env_file:
      - .env
    volumes:
      - ./chroma_db:/app/chroma_db
      - ./storage:/app/storage
    restart: unless-stopped
```

### Build + run with `docker` directly

```bash
cd rag-service
docker build -t rag-prn222 .
docker run --rm -p 8000:8000 --env-file .env rag-prn222
```

### Notes

* The image is based on `python:3.11-slim`.
* OS packages installed in the image: `build-essential`, `libgl1` (needed by
  some PyMuPDF / Chroma builds).
* The `.env` file is **not** copied into the image — it is supplied at
  runtime via `env_file` / `--env-file`.
* The first request that needs the embedding model triggers a ~2 GB download
  of `BAAI/bge-m3` into the container. Mount a HuggingFace cache volume if
  you want to reuse it across containers:

  ```yaml
  volumes:
    - hf-cache:/root/.cache/huggingface
  ```

### Cross-service file paths

The .NET MVC app saves uploaded files to the path configured in
`FileStorage:DocumentFolder` and then sends that path to the RAG service.
If the RAG service runs in Docker, the path it receives **must be valid
inside the container**, not on the host. Mount the same folder under the same
path in both environments, or change `FileStorage:DocumentFolder` to a path
that exists inside the container.

## ASP.NET Core 8 MVC

**TODO** — no Dockerfile is provided for the MVC project yet. To deploy it:

* IIS / Windows Server — publish with `dotnet publish -c Release` and host
  the result behind IIS.
* Linux / containers — generate a Dockerfile with
  `dotnet new dockerfile` or hand-write one based on
  `mcr.microsoft.com/dotnet/aspnet:8.0`.
* Azure App Service / AWS Elastic Beanstalk — both have first-class support
  for .NET 8.

Required runtime configuration (any deployment target):

| Setting | Provided via |
| --- | --- |
| `ConnectionStrings:DefaultConnection` | env var / `appsettings.{Environment}.json` |
| `RagService:BaseUrl` | URL of the deployed RAG service |
| `RagService:ApiKey` | matches the RAG service `API_KEY` |
| `FileStorage:DocumentFolder` | shared filesystem location |
| `AdminAccount:Email` / `Password` | admin login |

## Production hardening checklist (TODO)

These are **not** implemented in the current code — they are listed here so
the next contributor knows where to start.

* [ ] Hash passwords in `Accounts` (BCrypt / ASP.NET Identity) instead of
      storing them in plain text.
* [ ] Use HTTPS + HSTS in the RAG service (currently CORS is `*` and HTTP).
* [ ] Tighten `CORSMiddleware` `allow_origins` to the MVC origin only.
* [ ] Move SQL credentials out of `appsettings.json` (env vars / Key Vault).
* [ ] Add a real LLM provider implementation in
      [`rag-service/app/services/llm_service.py`](../rag-service/app/services/llm_service.py)
      (`openai` / `gemini` / `ollama` branches are scaffolded as
      `NotImplementedError`).
* [ ] Set up backups for `chroma_db/` and the SQL Server database.
* [ ] Add structured request logging + health checks behind a reverse proxy
      (nginx / Traefik).
