import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { notesService } from '../services/notesService';
import {
  NoteResponse,
  CreateNoteRequest,
  UpdateNoteRequest,
  SearchNotesRequest,
  APIError,
} from '../types';

// Query keys for React Query
export const noteKeys = {
  all: ['notes'] as const,
  lists: () => [...noteKeys.all, 'list'] as const,
  list: (filters: string) => [...noteKeys.lists(), filters] as const,
  details: () => [...noteKeys.all, 'detail'] as const,
  detail: (id: string) => [...noteKeys.details(), id] as const,
  search: (query: SearchNotesRequest) => [...noteKeys.all, 'search', query] as const,
  tags: () => [...noteKeys.all, 'tags'] as const,
};

/**
 * Hook to fetch all notes
 */
export const useNotes = () => {
  return useQuery({
    queryKey: noteKeys.lists(),
    queryFn: () => notesService.getAllNotes(),
    staleTime: 1000 * 60 * 5, // 5 minutes
    gcTime: 1000 * 60 * 10, // 10 minutes
  });
};

/**
 * Hook to fetch a single note by ID
 */
export const useNote = (id: string) => {
  return useQuery({
    queryKey: noteKeys.detail(id),
    queryFn: () => notesService.getNoteById(id),
    enabled: !!id,
    staleTime: 1000 * 60 * 5, // 5 minutes
  });
};

/**
 * Hook to search notes
 */
export const useSearchNotes = (searchQuery: SearchNotesRequest) => {
  return useQuery({
    queryKey: noteKeys.search(searchQuery),
    queryFn: () => notesService.searchNotes(searchQuery),
    enabled: !!(searchQuery.keyword || (searchQuery.tags && searchQuery.tags.length > 0)),
    staleTime: 1000 * 60 * 2, // 2 minutes
  });
};

/**
 * Hook to fetch all unique tags
 */
export const useTags = () => {
  return useQuery({
    queryKey: noteKeys.tags(),
    queryFn: () => notesService.getTags(),
    staleTime: 1000 * 60 * 10, // 10 minutes
  });
};

/**
 * Hook to create a new note
 */
export const useCreateNote = () => {
  const queryClient = useQueryClient();

  return useMutation<NoteResponse, APIError, CreateNoteRequest>({
    mutationFn: (noteData) => notesService.createNote(noteData),
    onSuccess: (newNote) => {
      // Update the notes list
      queryClient.setQueryData<NoteResponse[]>(noteKeys.lists(), (old) => {
        if (!old) return [newNote];
        return [newNote, ...old];
      });

      // Invalidate and refetch related queries
      queryClient.invalidateQueries({ queryKey: noteKeys.lists() });
      queryClient.invalidateQueries({ queryKey: noteKeys.tags() });
    },
  });
};

/**
 * Hook to update an existing note
 */
export const useUpdateNote = () => {
  const queryClient = useQueryClient();

  return useMutation<NoteResponse, APIError, { id: string; data: UpdateNoteRequest }>({
    mutationFn: ({ id, data }) => notesService.updateNote(id, data),
    onSuccess: (updatedNote) => {
      // Update the note in the list
      queryClient.setQueryData<NoteResponse[]>(noteKeys.lists(), (old) => {
        if (!old) return [updatedNote];
        return old.map((note) => (note.id === updatedNote.id ? updatedNote : note));
      });

      // Update the individual note cache
      queryClient.setQueryData(noteKeys.detail(updatedNote.id), updatedNote);

      // Invalidate related queries
      queryClient.invalidateQueries({ queryKey: noteKeys.lists() });
      queryClient.invalidateQueries({ queryKey: noteKeys.tags() });
    },
  });
};

/**
 * Hook to delete a note
 */
export const useDeleteNote = () => {
  const queryClient = useQueryClient();

  return useMutation<void, APIError, string>({
    mutationFn: (id) => notesService.deleteNote(id),
    onSuccess: (_, deletedId) => {
      // Remove the note from the list
      queryClient.setQueryData<NoteResponse[]>(noteKeys.lists(), (old) => {
        if (!old) return [];
        return old.filter((note) => note.id !== deletedId);
      });

      // Remove the individual note from cache
      queryClient.removeQueries({ queryKey: noteKeys.detail(deletedId) });

      // Invalidate related queries
      queryClient.invalidateQueries({ queryKey: noteKeys.lists() });
      queryClient.invalidateQueries({ queryKey: noteKeys.tags() });
    },
  });
};

/**
 * Hook to prefetch a note for performance
 */
export const usePrefetchNote = () => {
  const queryClient = useQueryClient();

  return (id: string) => {
    queryClient.prefetchQuery({
      queryKey: noteKeys.detail(id),
      queryFn: () => notesService.getNoteById(id),
      staleTime: 1000 * 60 * 5, // 5 minutes
    });
  };
};