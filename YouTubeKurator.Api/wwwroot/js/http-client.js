class HttpClient {
    constructor() {
        this.authService = new AuthService();
    }

    async request(url, options = {}) {
        const token = this.authService.getToken();
        if (token) {
            options.headers = options.headers || {};
            options.headers['Authorization'] = `Bearer ${token}`;
        }

        const response = await fetch(url, options);

        // Handle 401 Unauthorized - redirect to login
        if (response.status === 401) {
            this.authService.logout();
            window.location.href = '/login.html';
            throw new Error('Unauthorized');
        }

        return response;
    }

    async get(url) {
        return this.request(url, { method: 'GET' });
    }

    async post(url, body) {
        return this.request(url, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(body)
        });
    }

    async put(url, body) {
        return this.request(url, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(body)
        });
    }

    async delete(url) {
        return this.request(url, { method: 'DELETE' });
    }
}

// Global instance
const httpClient = new HttpClient();
