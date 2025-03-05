# special docker that helps to restore packages efficiently
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS restoreBackend
ENV PATH="${PATH}:/root/.dotnet/tools"
RUN dotnet tool install --global --no-cache dotnet-subset
WORKDIR /restore
COPY ../src/backend /restore/backend
RUN dotnet subset restore backend/ExprCalc/ExprCalc.csproj \
  --root-directory /restore --output restore_subset/

# Build backend
FROM mcr.microsoft.com/dotnet/sdk:9.0 as buildBackend

COPY --from=restoreBackend /restore/restore_subset /src/backend
RUN dotnet restore -r linux-x64 /src/backend

COPY ../src/backend /src/backend
RUN dotnet publish -c Release -r linux-x64 --self-contained false -o /publish/app /src/backend/ExprCalc \
    && rm -f /publish/app/*.pdb \
    && rm -f /publish/app/appsettings.json
    && rm -f /publish/app/appsettings.Development.json
    && rm -f /publish/app/wwwroot/*

COPY ./appsettings.json /publish/app/


# Build frontend
FROM node:23-alpine AS buildFrontend

COPY ../src/frontend/package.json ../src/frontend/yarn.lock /src/frontend
WORKDIR /src/frontend/
RUN yarn install

COPY ../src/frontend .
RUN yarn build


# Final image
FROM mcr.microsoft.com/dotnet/aspnet:9.0-bookworm-slim

COPY --from=buildBackend /publish /home
WORKDIR /home/app/

COPY --from=buildFrontend /src/frontend/dist /home/app/wwwroot/

EXPOSE 80
ENTRYPOINT ["/home/app/ExprCalc"]
CMD ["--config /home/app/appsettings.json"]