// API Response Types
export interface NoteResponse {
  id: string;
  title: string;
  content: string;
  tags: string[];
  createdAt: string;
  updatedAt: string;
}

// API Request Types
export interface CreateNoteRequest {
  title: string;
  content: string;
  tags?: string[];
}

export interface UpdateNoteRequest {
  id: string;
  title: string;
  content: string;
  tags?: string[];
}

export interface SearchNotesRequest {
  keyword?: string;
  tags?: string[];
}

// Component Props Types
export interface NoteCardProps {
  note: NoteResponse;
  onEdit?: (note: NoteResponse) => void;
  onDelete?: (id: string) => void;
  onClick?: (note: NoteResponse) => void;
}

export interface NoteFormProps {
  note?: Partial<NoteResponse>;
  onSubmit: (data: CreateNoteRequest | UpdateNoteRequest) => void;
  onCancel?: () => void;
  isLoading?: boolean;
  isEdit?: boolean;
}

export interface SearchBarProps {
  onSearch: (query: SearchNotesRequest) => void;
  initialKeyword?: string;
  initialTags?: string[];
  isLoading?: boolean;
}

export interface TagInputProps {
  tags: string[];
  onChange: (tags: string[]) => void;
  placeholder?: string;
  className?: string;
}

// State Management Types
export interface NotesState {
  notes: NoteResponse[];
  selectedNote: NoteResponse | null;
  searchQuery: SearchNotesRequest;
  isLoading: boolean;
  error: string | null;
}

export interface NotesActions {
  setNotes: (notes: NoteResponse[]) => void;
  addNote: (note: NoteResponse) => void;
  updateNote: (note: NoteResponse) => void;
  deleteNote: (id: string) => void;
  setSelectedNote: (note: NoteResponse | null) => void;
  setSearchQuery: (query: SearchNotesRequest) => void;
  setLoading: (loading: boolean) => void;
  setError: (error: string | null) => void;
  clearError: () => void;
}

// API Error Types
export interface APIError {
  message: string;
  status?: number;
  traceId?: string;
}

// Form Types
export interface NoteFormData {
  title: string;
  content: string;
  tags: string[];
}

// Route Params
export interface NoteRouteParams {
  id: string;
}

// Utility Types
export type LoadingState = 'idle' | 'loading' | 'success' | 'error';

export interface PaginationParams {
  page?: number;
  limit?: number;
}

export interface SearchFilters {
  keyword?: string;
  tags?: string[];
  sortBy?: 'createdAt' | 'updatedAt' | 'title';
  sortOrder?: 'asc' | 'desc';
}