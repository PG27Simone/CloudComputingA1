import jwt from 'jsonwebtoken'
import 'dotenv/config';


function verifyToken(req, res, next) {
    const authHeader = req.headers["authorization"];
    if (!authHeader) {
        return res.status(401).json({ message: "No token provided" });
    }

    //split with a space and take second part of the array
    const token = authHeader.split(" ")[1]
    jwt.verify(token, process.env.JWT_SECRET, (err, decoded) => {
        if (err) {
            return res.status(401).json({ message: "Invalid token" });
        }
        req.user = decoded;
        next();
    });
}

export default verifyToken;