import {
    createContext,
    useContext,
    useEffect,
    useState,
    type ReactNode,
} from "react";
import { api } from "../../api/axios";

type LoginErrors = {
    username?: string;
    password?: string;
    general?: string;
};

type AuthContextType = {
    isAuthenticated: boolean;
    isLoading: boolean;
    login: (username: string, password: string) => Promise<LoginErrors | null>;
    logout: () => Promise<void>;
};

const AuthContext = createContext<AuthContextType | null>(null);

export const AuthProvider = ({ children }: { children: ReactNode }) => {
    const [isAuthenticated, setIsAuthenticated] = useState(false);
    const [isLoading, setIsLoading] = useState(true);

    useEffect(() => {
        const checkAuth = async () => {
            try {
                await api.get("/admin/auth/me");
                setIsAuthenticated(true);
            } catch {
                setIsAuthenticated(false);
            } finally {
                setIsLoading(false);
            }
        };

        checkAuth();
    }, []);

    const login = async (
        username: string,
        password: string
    ): Promise<LoginErrors | null> => {
        try {
            await api.post("/admin/auth/login", { username, password });
            setIsAuthenticated(true);
            return null;
        } catch (err: any) {
            if (err.response?.status === 400) {
                const errors: LoginErrors = {};

                const validationErrors = err.response.data;

                validationErrors.forEach((e: any) => {
                    if (e.propertyName === "Username") {
                        errors.username = e.errorMessage;
                    }
                    if (e.propertyName === "Password") {
                        errors.password = e.errorMessage;
                    }
                });

                return errors;
            }

            if (err.response?.status === 401) {
                return { general: err.response.data };
            }

            return { general: "Ошибка сервера" };
        }
    };

    const logout = async () => {
        await api.post("/admin/auth/logout");
        setIsAuthenticated(false);
    };

    return (
        <AuthContext.Provider value={{ isAuthenticated, isLoading, login, logout }}>
            {children}
        </AuthContext.Provider>
    );
};

export const useAuth = () => {
    const ctx = useContext(AuthContext);
    if (!ctx) throw new Error("AuthContext missing");
    return ctx;
};
