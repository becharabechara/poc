import api from './api';
import {
  NoteResponse,
  CreateNoteRequest,
  UpdateNoteRequest,
  SearchNotesRequest,
  PaginationParams,
} from '../types';

class NotesService {
  private readonly endpoint = '/notes';

  /**
   * Get all notes with optional pagination
   */
  async getAllNotes(params?: PaginationParams): Promise<NoteResponse[]> {
    const response = await api.get<NoteResponse[]>(this.endpoint, { params });
    return response.data;
  }

  /**
   * Get a single note by ID
   */
  async getNoteById(id: string): Promise<NoteResponse> {
    const response = await api.get<NoteResponse>(`${this.endpoint}/${id}`);
    return response.data;
  }

  /**
   * Create a new note
   */
  async createNote(noteData: CreateNoteRequest): Promise<NoteResponse> {
    const response = await api.post<NoteResponse>(this.endpoint, noteData);
    return response.data;
  }

  /**
   * Update an existing note
   */
  async updateNote(id: string, noteData: UpdateNoteRequest): Promise<NoteResponse> {
    const response = await api.put<NoteResponse>(`${this.endpoint}/${id}`, noteData);
    return response.data;
  }

  /**
   * Delete a note by ID
   */
  async deleteNote(id: string): Promise<void> {
    await api.delete(`${this.endpoint}/${id}`);
  }

  /**
   * Search notes with keyword and/or tags
   */
  async searchNotes(searchParams: SearchNotesRequest): Promise<NoteResponse[]> {
    const params: any = {};
    
    if (searchParams.keyword) {
      params.keyword = searchParams.keyword;
    }
    
    if (searchParams.tags && searchParams.tags.length > 0) {
      params.tags = searchParams.tags.join(',');
    }

    const response = await api.get<NoteResponse[]>(`${this.endpoint}/search`, { params });
    return response.data;
  }

  /**
   * Get all unique tags from notes
   */
  async getTags(): Promise<string[]> {
    const response = await api.get<string[]>(`${this.endpoint}/tags`);
    return response.data;
  }

  /**
   * Check if the API is healthy
   */
  async healthCheck(): Promise<boolean> {
    try {
      await api.get('/health');
      return true;
    } catch {
      return false;
    }
  }
}

// Export singleton instance
export const notesService = new NotesService();
export default notesService;