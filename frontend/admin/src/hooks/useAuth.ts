import { useState } from 'react';
import { adminApi } from '../services/adminApi';

export function useAuth() {
  const [token, setToken] = useState(() => localStorage.getItem('auth_token'));

  const login = async (username: string, password: string) => {
    const result = await adminApi.login(username, password);
    localStorage.setItem('auth_token', result.token);
    setToken(result.token);
  };

  const logout = () => {
    localStorage.removeItem('auth_token');
    setToken(null);
  };

  return { isAuthenticated: !!token, login, logout };
}
