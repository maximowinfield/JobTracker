# ---------- 1) Build React (Vite) ----------
FROM node:20-alpine AS web-build
WORKDIR /src/jobtracker-web

COPY jobtracker-web/package.json jobtracker-web/package-lock.json* ./
RUN npm ci

COPY jobtracker-web/ ./

ARG VITE_API_URL=/api
ARG VITE_BASE=/
ENV VITE_API_URL=${VITE_API_URL}
ENV VITE_BASE=${VITE_BASE}

RUN echo "VITE_API_URL=${VITE_API_URL}" > .env.production \
    && echo "VITE_BASE=${VITE_BASE}" >> .env.production

RUN npm run build


# ---------- 2) Build & publish .NET API ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS api-build
WORKDIR /src

# Your csproj is in api/JobTracker.Api/JobTracker.Api.csproj
COPY api/JobTracker.Api/JobTracker.Api.csproj ./api/JobTracker.Api/
RUN dotnet restore ./api/JobTracker.Api/JobTracker.Api.csproj

COPY api/ ./api/
RUN dotnet publish ./api/JobTracker.Api/JobTracker.Api.csproj -c Release -o /out


# ---------- 3) Final runtime image ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT:-10000}

COPY --from=api-build /out ./
COPY --from=web-build /src/jobtracker-web/dist ./wwwroot

EXPOSE 10000
ENTRYPOINT ["dotnet", "JobTracker.Api.dll"]
