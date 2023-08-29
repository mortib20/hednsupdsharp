from alpine:latest as build
WORKDIR /build
COPY . .
RUN apk add --no-cache dotnet7-sdk
RUN dotnet publish -c Release -o ./bin .

from alpine:latest as main
WORKDIR /hednsupd
COPY --from=build /build/bin/ /hednsupd
RUN apk add --no-cache aspnetcore7-runtime

ENV HEDNSUPD_HOSTNAME "HOSTNAME NOT SET"
ENV HEDNSUPD_KEY "KEY NOT SET"
ENV HEDNSUPD_DELAY 600

CMD ["./hednsupd"]