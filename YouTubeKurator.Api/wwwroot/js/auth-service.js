class AuthService {
    constructor() {
        this.baseUrl = '/api/auth';
    }

    async sendAuthCode(email) {
        try {
            const response = await fetch(`${this.baseUrl}/start`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ email })
            });
            return await response.json();
        } catch (error) {
            console.error('Failed to send auth code:', error);
            throw error;
        }
    }

    async verifyCode(email, code) {
        try {
            const response = await fetch(`${this.baseUrl}/verify`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ email, code })
            });
            const data = await response.json();
            if (data.token) {
                localStorage.setItem('jwtToken', data.token);
                localStorage.setItem('userId', data.userId);
                localStorage.setItem('email', data.email);
            }
            return data;
        } catch (error) {
            console.error('Failed to verify code:', error);
            throw error;
        }
    }

    getToken() {
        return localStorage.getItem('jwtToken');
    }

    getUserId() {
        return localStorage.getItem('userId');
    }

    getEmail() {
        return localStorage.getItem('email');
    }

    logout() {
        localStorage.removeItem('jwtToken');
        localStorage.removeItem('userId');
        localStorage.removeItem('email');
    }

    isLoggedIn() {
        return !!this.getToken();
    }
}
