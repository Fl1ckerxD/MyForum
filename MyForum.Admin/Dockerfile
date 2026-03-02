FROM node:25-alpine AS build

WORKDIR /app

COPY package.json package-lock.json ./
RUN npm ci

COPY . .
RUN npm run build

FROM alpine:latest
WORKDIR /output
COPY --from=build /app/dist ./dist

CMD ["sh", "-c", "cp -r dist/* /dist-output/"]