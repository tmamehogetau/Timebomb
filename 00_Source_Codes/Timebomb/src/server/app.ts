import express from "express";
import path from "node:path";

export function createApp(clientDist: string): express.Express {
  const app = express();

  app.get("/healthz", (_req, res) => {
    res.status(200).json({ ok: true });
  });

  app.use(express.static(clientDist));
  app.use((_req, res) => {
    res.sendFile(path.join(clientDist, "index.html"), (error) => {
      if (error && !res.headersSent) {
        res.status(200).send("クライアント未ビルド。npm run dev で開発してください。");
      }
    });
  });

  return app;
}
