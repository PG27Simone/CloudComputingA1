import 'dotenv/config';
import express from 'express';
import cors from 'cors';
import path from 'path';
import fs from 'fs';
import { fileURLToPath } from 'url';

import router from "./authRoutes.js";
import verifyToken from "./authMiddleware.js";
import pool from "./db.js";

const app = express();
app.use(cors());
app.use(express.json());

app.use("/api/auth", router);

const __filename = fileURLToPath(import.meta.url); // get the resolved path to the file
const __dirname = path.dirname(__filename); // get the name of the directory

const PATH = path.join(__dirname, "events.json");

const UNITY_STREAMING_ASSETS = path.join(__dirname, "..", "Assets", "StreamingAssets");
const SAVEPATH = path.join(UNITY_STREAMING_ASSETS, "save.json");

app.get("/api/protected", verifyToken, async (req, res) => {
    try {
        const [rows] = await pool.query("SELECT id, username, email FROM users WHERE id = ?",
            [req.user.userId]);
        const user = rows[0];
        res.json({ message: "This is hidden data", user });
    } catch(err) {
        console.error(err);
        res.status(500).json({ message: "Internal server error" });
    }
});

//events
app.post('/telemetry', (req, res) => {
    try {
        const eventData = req.body;

        let existingEvents = [];
        if (fs.existsSync(PATH)) {
            const rawData = fs.readFileSync(PATH, "utf-8");
            if (rawData.length > 0) {
                existingEvents = JSON.parse(rawData);
            }
        }

        eventData.timestamp = new Date().toISOString();
        existingEvents.push(eventData);

        fs.writeFileSync(PATH, JSON.stringify(existingEvents, null, 2));

        return res.status(200).json({ message: "Data Stored" });
    } catch (error) {
        return res.status(500).json({ error: "Internal Server Error" });
    }
});

//save state
app.post('/save', (req, res) => {
    try {
        const eventData = req.body;

        if (!fs.existsSync(UNITY_STREAMING_ASSETS)) {
            fs.mkdirSync(UNITY_STREAMING_ASSETS, { recursive: true });
        }

        eventData.timestamp = new Date().toISOString();

        fs.writeFileSync(SAVEPATH, JSON.stringify(eventData, null, 2));

        return res.status(200).json({ message: "Data Stored" });
    } catch (error) {
        return res.status(500).json({ error: "Internal Server Error" });
    }
});

const PORT = process.env.PORT || 3000;
app.listen(PORT, () => {
    console.log(`Server running on port ${PORT}`);
});