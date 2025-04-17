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

const EVENTS_FOLDER = path.join(__dirname, 'Events');

const SAVE_FOLDER = path.join(__dirname, 'Saves');

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
        const incomingEvent = req.body;

        if (!incomingEvent.keys || !incomingEvent.values) {
            return res.status(400).json({ error: "Invalid format: expecting keys and values arrays." });
        }

        const usernameIndex = incomingEvent.keys.indexOf("username");
        if (usernameIndex === -1) {
            return res.status(400).json({ error: "Missing 'username' key in telemetry data." });
        }

        const username = incomingEvent.values[usernameIndex];

        if (!incomingEvent.timestamp) {
            incomingEvent.timestamp = new Date().toISOString();
        }

        if (!fs.existsSync(EVENTS_FOLDER)) {
            fs.mkdirSync(EVENTS_FOLDER, { recursive: true });
        }

        const filePath = path.join(EVENTS_FOLDER, `${username}_events.json`);
        let allEvents = [];

        if (fs.existsSync(filePath)) {
            const raw = fs.readFileSync(filePath, "utf-8");
            if (raw.trim().length > 0) {
                allEvents = JSON.parse(raw);
            }
        }

        allEvents.push(incomingEvent);
        fs.writeFileSync(filePath, JSON.stringify(allEvents, null, 2));


        return res.status(200).json({ message: "Data Stored" });
    } catch (error) {
        return res.status(500).json({ error: "Internal Server Error" });
    }
});

//save state
app.post('/save', (req, res) => {
    try {
        const eventData = req.body;

        //if no username, error
        if (!eventData.Name) {
            return res.status(400).json({ error: "Missing username in request data." });
        }

        //make sure save folder exists
        if (!fs.existsSync(SAVE_FOLDER)) {
            fs.mkdirSync(SAVE_FOLDER, { recursive: true });
        }

        //define personalized save file
        const userSavePath = path.join(SAVE_FOLDER, `${eventData.Name}_save.json`);

        eventData.timestamp = new Date().toISOString();

        fs.writeFileSync(userSavePath, JSON.stringify(eventData, null, 2));

        return res.status(200).json({ message: "Data Stored" });
    } catch (error) {
        return res.status(500).json({ error: "Internal Server Error" });
    }
});

const PORT = process.env.PORT || 3000;
app.listen(PORT, () => {
    console.log(`Server running on port ${PORT}`);
});