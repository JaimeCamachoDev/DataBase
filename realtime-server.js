const express = require('express');
const bodyParser = require('body-parser');
const { Server } = require('ws');

const app = express();
app.use(bodyParser.json());

const server = app.listen(process.env.PORT || 3000, () => {
  console.log('Realtime server running on port', server.address().port);
});

const wss = new Server({ server });
let lastPayload = null;

wss.on('connection', ws => {
  // Enviamos el último estado al nuevo cliente si existe
  if (lastPayload) ws.send(lastPayload);
});

app.post('/sync', (req, res) => {
  const payload = JSON.stringify(req.body);
  lastPayload = payload;
  // Emitimos el nuevo estado a todos los clientes conectados
  wss.clients.forEach(client => {
    if (client.readyState === 1) {
      client.send(payload);
    }
  });
  res.status(200).send('ok');
});
