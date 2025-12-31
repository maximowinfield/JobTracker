# ============================
# 1) Build Frontend (Vite)
# ============================
FROM node:20-alpine AS web-build
WORKDIR /app/jobtracker-web

# Install frontend deps
COPY jobtracker-web/package.json jobtracker-web/package-lock.json ./
RUN npm ci

# Copy frontend source
COPY jobtracker-web/ ./

# Build-time envs for single-service deploy
ARG VITE_API_URL=/api
ARG VITE_BASE=/
ENV VITE_API_URL=${VITE_API_URL}
ENV VITE_BASE=${VITE_BASE}

RUN echo "VITE_API_URL=${VITE_API_URL}" > .env.production \
    && echo "VITE_BASE=${VITE_BASE}" >> .env.production

# Build frontend
RUN npm run build


# ============================
# 2) Build & Publish .NET API
# ============================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS api-build
WORKDIR /app

# Copy only csproj first (layer caching)
COPY api/JobTracker.Api/JobTracker.Api.csproj api/JobTracker.Api/
RUN dotnet restore api/JobTracker.Api/JobTracker.Api.csproj

# Copy the rest of the API
COPY api/ api/
RUN dotnet publish api/JobTracker.Api/JobTracker.Api.csproj -c Release -o /out


# ============================
# 3) Final Runtime Image
# ============================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Render port binding
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT:-10000}

# Copy published API
COPY --from=api-build /out ./

# Copy built frontend into wwwroot
COPY --from=web-build /app/jobtracker-web/dist ./wwwroot

EXPOSE 10000

ENTRYPOINT ["dotnet", "JobTracker.Api.dll"]
