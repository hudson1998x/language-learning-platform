FROM mcr.microsoft.com/dotnet/sdk:10.0
WORKDIR /src

RUN apt-get update && \
    apt-get install -y nodejs npm && \
    rm -rf /var/lib/apt/lists/*

COPY package.json package-lock.json ./
RUN npm ci

COPY . .

RUN npm run esbuild:build
RUN dotnet restore

ENV OLLAMA_URL=http://ollama:11434

EXPOSE 8080

ENTRYPOINT ["dotnet", "run", "--project", "App/Code/Core/Application"]
