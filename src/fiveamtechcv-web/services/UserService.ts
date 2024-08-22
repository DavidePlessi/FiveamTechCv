import type {ICreateUser, ILoginUser} from "~/entities/entities";
import {API_URL} from "~/config";

export class UserService {
  constructor() {
    this.url = `${API_URL}/user`
    this.token = null;
    this.checkToken();
  }
  
  checkToken() {
    this.token = localStorage.getItem('token');
    if(this.token) {
      this.setToken(this.token);
      //await this.validateToken();
      
    }
  }
  
  setToken(token: string) {
    this.token = token;
    axios.defaults.headers.common['Authorization'] = `Bearer ${token}`;
    localStorage.setItem('token', token);
  }
  
  removeToken() {
    this.token = null;
    axios.defaults.headers.common['Authorization'] = '';
    localStorage.removeItem('token');
  }
  
  async createUser(data: ICreateUser): Promise<string> {
    const response = await axios.post(`${this.url}/create-user`, data);
    return response.data;    
  }
  
  async login(data: ILoginUser): Promise<string> {
    const response = await axios.post(`${this.url}/login`, data);
    const token = response.data;
    this.setToken(token);
    return token;
  }

  async validateToken(): Promise<string> {
    const response = await axios.post(`${this.url}/validate-token`, data);
    const token = response.data;
    this.setToken(token);
    return token;
  }
}