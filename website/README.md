# Radius Website

Static marketing site for `radius.northwindlab.website`.

## Local Preview

The site is static. You can preview it from the repository root with:

```powershell
python -m http.server 8080 -d website
```

Then open `http://localhost:8080`.

## Docker Preview

Build and run the production container locally:

```bash
docker compose up -d --build
```

Then open `http://localhost:8080`.

Stop it with:

```bash
docker compose down
```

## Oracle Cloudflare Tunnel Deployment

Recommended shape on the Oracle instance:

1. Copy the `website` folder to the server, for example `~/radius/website`.
2. Run the Docker container on the existing `aura-home_default` Docker network.
3. Point the existing Cloudflare Tunnel public hostname at `http://radius-site:8080`.

Example server commands:

```bash
mkdir -p ~/radius
cd ~/radius/website
docker compose -f docker-compose.oracle.yml up -d --build
```

For the existing remotely-managed `aura-home-tunnel-1` container, add this public hostname in Cloudflare Zero Trust:

- Hostname: `radius.northwindlab.website`
- Service: `http://radius-site:8080`

If you later switch to a local `cloudflared` config file, the equivalent ingress is:

```yaml
tunnel: <your-tunnel-id>
credentials-file: /etc/cloudflared/<your-tunnel-id>.json

ingress:
  - hostname: radius.northwindlab.website
    service: http://localhost:8080
  - service: http_status:404
```

## Suggested Security Headers

Radius does not need any third-party scripts or remote assets, so the site can run with tight headers.

These are already included in `nginx.conf`:

```nginx
add_header Content-Security-Policy "default-src 'self'; img-src 'self'; style-src 'self'; base-uri 'self'; frame-ancestors 'none'; form-action 'none'" always;
add_header X-Content-Type-Options "nosniff" always;
add_header Referrer-Policy "strict-origin-when-cross-origin" always;
add_header Permissions-Policy "camera=(), microphone=(), geolocation=(), payment=()" always;
```
