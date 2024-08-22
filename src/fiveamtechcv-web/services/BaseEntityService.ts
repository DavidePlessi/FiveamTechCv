import {API_URL} from "~/config";
import type {IBaseEntity, IBaseEntityDto, IBaseEntityFilter} from "~/entities/entities";

export class BaseEntityService<
  TEntity extends IBaseEntity, 
  TFilter extends IBaseEntityFilter, 
  TDto extends IBaseEntityDto
> {
  constructor(path: string) {
    this.url = `${API_URL}/api/${path}`;
  }
  
  async getById(id: string): Promise<TEntity> { 
    const response = await axios.get(`${this.url}/${id}`);
    return response.data;
  }
  
  async getList(filter: TFilter): Promise<TEntity[]> {
    const response = await axios.get(`${this.url}`, { params: filter });
    return response.data;
    
  }
  
  async create(dto: TDto): Promise<string> {
    const response = await axios.post(`${this.url}`, dto);
    return response.data;
  }
  
  async update(id: string, dto: TDto): Promise<TEntity> {
    const response = await axios.put(`${this.url}/${id}`, dto);
    return response.data;
  }
  
  async delete(id: string): Promise<void> {
    const result = await axios.delete(`${this.url}/${id}`);
    return result.data;
  }
  
  
}