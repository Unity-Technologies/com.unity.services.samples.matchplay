const WebSocket = require('ws');

const wss = new WebSocket.Server({ port: 8080 });

console.log('Reality Sync Mock Server started on ws://localhost:8080');

const nodes = [
    { id: 'Node_Alpha', variance: 0.1, state: 'NOW' },
    { id: 'Node_Beta', variance: 0.5, state: 'NOW' },
    { id: 'Node_Gamma', variance: 0.9, state: 'VOID' }
];

wss.on('connection', (ws) => {
    console.log('Client connected to the Void grid.');

    // Send initial state
    const initialPayload = {
        type: 'STATE_SYNC',
        nodes: nodes
    };
    ws.send(JSON.stringify(initialPayload));

    // Simulate periodic updates
    const interval = setInterval(() => {
        nodes.forEach(node => {
            // Randomly fluctuate variance
            node.variance = Math.max(0, Math.min(1, node.variance + (Math.random() - 0.5) * 0.1));

            // Randomly flip state if variance is high
            if (node.variance > 0.8 && Math.random() > 0.95) {
                node.state = node.state === 'NOW' ? 'VOID' : 'NOW';
            }
        });

        const updatePayload = {
            type: 'STATE_SYNC',
            nodes: nodes
        };

        if (ws.readyState === WebSocket.OPEN) {
            ws.send(JSON.stringify(updatePayload));
        }
    }, 2000);

    ws.on('message', (message) => {
        try {
            const data = JSON.parse(message);
            console.log(`Received pulse: Node=${data.nodeId}, Delta=${data.deltaVariance}`);

            // Apply pulse to local state
            const node = nodes.find(n => n.id === data.nodeId);
            if (node) {
                node.variance = Math.max(0, node.variance + data.deltaVariance);
                if (node.variance < 0.2) node.state = 'NOW';
            }
        } catch (e) {
            console.error('Failed to parse client message:', e.message);
        }
    });

    ws.on('close', () => {
        console.log('Client disconnected.');
        clearInterval(interval);
    });
});
