import React, { useState } from 'react';
import { Link } from 'react-router-dom';
import { Search, Plus, FileText, Calendar, Tag } from 'lucide-react';
import { useNotes } from '../hooks/useNotes';
import { formatDate, cn } from '../utils';
import { NoteResponse, SearchNotesRequest } from '../types';

const HomePage: React.FC = () => {
  const { data: notes, isLoading, error } = useNotes();
  const [searchQuery, setSearchQuery] = useState<SearchNotesRequest>({});

  const filteredNotes = notes?.filter((note) => {
    if (!searchQuery.keyword && (!searchQuery.tags || searchQuery.tags.length === 0)) {
      return true;
    }

    const matchesKeyword = !searchQuery.keyword || 
      note.title.toLowerCase().includes(searchQuery.keyword.toLowerCase()) ||
      note.content.toLowerCase().includes(searchQuery.keyword.toLowerCase());

    const matchesTags = !searchQuery.tags || searchQuery.tags.length === 0 ||
      searchQuery.tags.some(tag => note.tags.includes(tag));

    return matchesKeyword && matchesTags;
  });

  if (isLoading) {
    return (
      <div className="space-y-6">
        <div className="flex justify-between items-center">
          <h1 className="text-3xl font-bold text-gray-900">My Notes</h1>
          <Link
            to="/create"
            className="btn-primary h-10 px-4"
          >
            <Plus className="h-4 w-4 mr-2" />
            New Note
          </Link>
        </div>
        
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
          {[...Array(6)].map((_, i) => (
            <div key={i} className="card p-6 animate-pulse">
              <div className="h-5 bg-gray-200 rounded mb-3"></div>
              <div className="space-y-2">
                <div className="h-4 bg-gray-200 rounded"></div>
                <div className="h-4 bg-gray-200 rounded w-3/4"></div>
              </div>
              <div className="flex justify-between items-center mt-4">
                <div className="h-3 bg-gray-200 rounded w-1/3"></div>
                <div className="flex space-x-1">
                  <div className="h-5 w-12 bg-gray-200 rounded-full"></div>
                  <div className="h-5 w-16 bg-gray-200 rounded-full"></div>
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="text-center py-12">
        <FileText className="h-12 w-12 text-gray-400 mx-auto mb-4" />
        <h2 className="text-xl font-semibold text-gray-900 mb-2">
          Failed to load notes
        </h2>
        <p className="text-gray-600 mb-4">
          {error.message || 'Something went wrong while loading your notes.'}
        </p>
        <button
          onClick={() => window.location.reload()}
          className="btn-primary"
        >
          Try Again
        </button>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:justify-between sm:items-center gap-4">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">My Notes</h1>
          <p className="text-gray-600 mt-1">
            {notes?.length || 0} {notes?.length === 1 ? 'note' : 'notes'} total
          </p>
        </div>
        <Link
          to="/create"
          className="btn-primary h-10 px-4 w-fit"
        >
          <Plus className="h-4 w-4 mr-2" />
          New Note
        </Link>
      </div>

      {/* Quick Search */}
      <div className="card p-4">
        <div className="flex items-center space-x-4">
          <div className="flex-1 relative">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-gray-400" />
            <input
              type="text"
              placeholder="Quick search notes..."
              className="input pl-10"
              value={searchQuery.keyword || ''}
              onChange={(e) => setSearchQuery({ ...searchQuery, keyword: e.target.value })}
            />
          </div>
          <Link
            to="/search"
            className="btn-secondary h-10 px-4"
          >
            Advanced Search
          </Link>
        </div>
      </div>

      {/* Notes Grid */}
      {filteredNotes?.length === 0 ? (
        <div className="text-center py-12">
          {notes?.length === 0 ? (
            <>
              <FileText className="h-12 w-12 text-gray-400 mx-auto mb-4" />
              <h2 className="text-xl font-semibold text-gray-900 mb-2">
                No notes yet
              </h2>
              <p className="text-gray-600 mb-6">
                Get started by creating your first note.
              </p>
              <Link to="/create" className="btn-primary">
                <Plus className="h-4 w-4 mr-2" />
                Create Your First Note
              </Link>
            </>
          ) : (
            <>
              <Search className="h-12 w-12 text-gray-400 mx-auto mb-4" />
              <h2 className="text-xl font-semibold text-gray-900 mb-2">
                No notes found
              </h2>
              <p className="text-gray-600 mb-6">
                Try adjusting your search criteria or create a new note.
              </p>
              <div className="flex justify-center space-x-4">
                <button
                  onClick={() => setSearchQuery({})}
                  className="btn-secondary"
                >
                  Clear Search
                </button>
                <Link to="/create" className="btn-primary">
                  <Plus className="h-4 w-4 mr-2" />
                  Create Note
                </Link>
              </div>
            </>
          )}
        </div>
      ) : (
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
          {filteredNotes?.map((note) => (
            <NoteCard key={note.id} note={note} />
          ))}
        </div>
      )}
    </div>
  );
};

interface NoteCardProps {
  note: NoteResponse;
}

const NoteCard: React.FC<NoteCardProps> = ({ note }) => {
  return (
    <Link
      to={`/notes/${note.id}`}
      className="card p-6 hover:shadow-md transition-shadow cursor-pointer group"
    >
      <div className="flex justify-between items-start mb-3">
        <h3 className="font-semibold text-gray-900 group-hover:text-primary-600 transition-colors line-clamp-2">
          {note.title}
        </h3>
      </div>
      
      <p className="text-gray-600 text-sm line-clamp-3 mb-4">
        {note.content || 'No content'}
      </p>
      
      <div className="flex justify-between items-end">
        <div className="flex items-center text-xs text-gray-500">
          <Calendar className="h-3 w-3 mr-1" />
          {formatDate(note.updatedAt)}
        </div>
        
        {note.tags.length > 0 && (
          <div className="flex flex-wrap gap-1 max-w-32">
            {note.tags.slice(0, 2).map((tag, index) => (
              <span key={index} className="tag text-xs">
                {tag}
              </span>
            ))}
            {note.tags.length > 2 && (
              <span className="text-xs text-gray-500">
                +{note.tags.length - 2}
              </span>
            )}
          </div>
        )}
      </div>
    </Link>
  );
};

export default HomePage;