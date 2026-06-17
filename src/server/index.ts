import { createServer } from "node:http";
import path from "node:path";
import { fileURLToPath } from "node:url";
import { createApp } from "./app.js";
import { attachWebSocketServer } from "./websocket.js";

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const clientDist = path.resolve(__dirname, "../../dist");
const app = createApp(clientDist);
const server = createServer(app);
const port = Number(process.env.PORT ?? 3000);

attachWebSocketServer(server);

server.listen(port, "0.0.0.0", () => {
  console.log(`server listening on 0.0.0.0:${port}`);
});
