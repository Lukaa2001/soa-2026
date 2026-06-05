# Build context: back-end/followers-service
FROM golang:1.22 AS build
WORKDIR /app
COPY go.mod ./
COPY *.go ./
# go.sum is generated here since the module is built from source without a lockfile.
RUN go mod tidy
RUN CGO_ENABLED=0 GOOS=linux go build -o /followers .

FROM gcr.io/distroless/static-debian12
COPY --from=build /followers /followers
EXPOSE 8080
ENTRYPOINT ["/followers"]
