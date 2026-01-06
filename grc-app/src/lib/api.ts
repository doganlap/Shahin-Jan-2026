// API Configuration and Helper Functions
const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'https://portal.shahin-ai.com';

interface ApiResponse<T> {
  success: boolean;
  data?: T;
  message?: string;
  error?: string;
}

class ApiClient {
  private baseUrl: string;
  private token: string | null = null;

  constructor(baseUrl: string) {
    this.baseUrl = baseUrl;
    if (typeof window !== 'undefined') {
      this.token = localStorage.getItem('authToken');
    }
  }

  setToken(token: string) {
    this.token = token;
    if (typeof window !== 'undefined') {
      localStorage.setItem('authToken', token);
    }
  }

  clearToken() {
    this.token = null;
    if (typeof window !== 'undefined') {
      localStorage.removeItem('authToken');
    }
  }

  private async request<T>(endpoint: string, options: RequestInit = {}): Promise<ApiResponse<T>> {
    const headers: HeadersInit = {
      'Content-Type': 'application/json',
      ...options.headers,
    };

    if (this.token) {
      (headers as Record<string, string>)['Authorization'] = `Bearer ${this.token}`;
    }

    try {
      const response = await fetch(`${this.baseUrl}${endpoint}`, {
        ...options,
        headers,
      });

      const data = await response.json();

      if (!response.ok) {
        return { success: false, error: data.message || 'Request failed' };
      }

      return { success: true, data };
    } catch (error) {
      return { success: false, error: (error as Error).message };
    }
  }

  // Auth endpoints
  async login(email: string, password: string) {
    return this.request<{ token: string; user: any }>('/api/auth/login', {
      method: 'POST',
      body: JSON.stringify({ email, password }),
    });
  }

  async logout() {
    this.clearToken();
    return { success: true };
  }

  // Dashboard
  async getDashboard() {
    return this.request<any>('/api/dashboard/summary');
  }

  // Controls (Shahin MAP)
  async getControls(params?: { page?: number; size?: number; q?: string; category?: string }) {
    const query = new URLSearchParams(params as Record<string, string>).toString();
    return this.request<any>(`/api/controls${query ? `?${query}` : ''}`);
  }

  async getControl(id: string) {
    return this.request<any>(`/api/controls/${id}`);
  }

  async createControl(data: any) {
    return this.request<any>('/api/controls', {
      method: 'POST',
      body: JSON.stringify(data),
    });
  }

  async updateControl(id: string, data: any) {
    return this.request<any>(`/api/controls/${id}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    });
  }

  // Evidence (Shahin PROVE/VAULT)
  async getEvidence(params?: { page?: number; size?: number }) {
    const query = new URLSearchParams(params as Record<string, string>).toString();
    return this.request<any>(`/api/evidence${query ? `?${query}` : ''}`);
  }

  async uploadEvidence(formData: FormData) {
    return this.request<any>('/api/evidence', {
      method: 'POST',
      body: formData,
      headers: {}, // Let browser set content-type for FormData
    });
  }

  // Assessments
  async getAssessments() {
    return this.request<any>('/api/assessments');
  }

  async getAssessment(id: string) {
    return this.request<any>(`/api/assessments/${id}`);
  }

  // Plans
  async getPlans() {
    return this.request<any>('/api/plans');
  }

  async getPlan(id: string) {
    return this.request<any>(`/api/plans/${id}`);
  }

  // Reports (Shahin WATCH)
  async getReports() {
    return this.request<any>('/api/reports');
  }

  async generateReport(type: string, params?: any) {
    return this.request<any>('/api/reports/generate', {
      method: 'POST',
      body: JSON.stringify({ type, ...params }),
    });
  }

  // Risks
  async getRisks() {
    return this.request<any>('/api/risks');
  }

  // Audits
  async getAudits() {
    return this.request<any>('/api/audits');
  }

  // Shahin Modules
  async getShahinModule(module: 'map' | 'apply' | 'prove' | 'watch' | 'fix' | 'vault') {
    return this.request<any>(`/api/shahin/${module}`);
  }
}

export const api = new ApiClient(API_BASE_URL);
export default api;
