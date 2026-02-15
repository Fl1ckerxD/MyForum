import { useState } from "react";
import { useAuth } from "./AuthContext";
import { useNavigate } from "react-router-dom";

export const LoginPage = () => {
    const { login } = useAuth();
    const navigate = useNavigate();

    const [username, setUsername] = useState("");
    const [password, setPassword] = useState("");

    const [errors, setErrors] = useState<{
        username?: string;
        password?: string;
        general?: string;
    }>({});

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        const result = await login(username, password);

        if (!result) {
            navigate("/boards");
        } else {
            setErrors(result);
        }
    };

    return (
        <div style={{ maxWidth: 400, margin: "100px auto" }}>
            <h2>Admin Login</h2>

            <form onSubmit={handleSubmit}>
                <div>
                    <input
                        placeholder="Username"
                        value={username}
                        onChange={e => setUsername(e.target.value)}
                    />
                    {errors.username && (
                        <p style={{ color: "red" }}>{errors.username}</p>
                    )}
                </div>

                <div>
                    <input
                        type="password"
                        placeholder="Password"
                        value={password}
                        onChange={e => setPassword(e.target.value)}
                    />
                    {errors.password && (
                        <p style={{ color: "red" }}>{errors.password}</p>
                    )}
                </div>

                {errors.general && (
                    <p style={{ color: "red" }}>{errors.general}</p>
                )}

                <button type="submit">Login</button>
            </form>
        </div>
    );
};
