import React, { useState } from 'react';
import { Search, Tag, X, FileText } from 'lucide-react';
import { useSearchNotes, useTags } from '../hooks/useNotes';
import { SearchNotesRequest } from '../types';
import { Link } from 'react-router-dom';
import { formatDate } from '../utils';

const SearchPage: React.FC = () => {
  const [searchQuery, setSearchQuery] = useState<SearchNotesRequest>({
    keyword: '',
    tags: [],
  });
  const [tagInput, setTagInput] = useState('');
  
  const { data: availableTags } = useTags();
  const { data: searchResults, isLoading, error } = useSearchNotes(searchQuery);

  const hasActiveSearch = searchQuery.keyword || (searchQuery.tags && searchQuery.tags.length > 0);

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    // Search is automatically triggered by useSearchNotes when searchQuery changes
  };

  const addTag = (tag: string) => {
    if (!searchQuery.tags?.includes(tag)) {
      setSearchQuery({
        ...searchQuery,
        tags: [...(searchQuery.tags || []), tag],
      });
    }
  };

  const removeTag = (tagToRemove: string) => {
    setSearchQuery({
      ...searchQuery,
      tags: searchQuery.tags?.filter(tag => tag !== tagToRemove) || [],
    });
  };

  const addCustomTag = () => {
    const tag = tagInput.trim().toLowerCase();
    if (tag) {
      addTag(tag);
      setTagInput('');
    }
  };

  const handleTagInputKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter') {
      e.preventDefault();
      addCustomTag();
    }
  };

  const clearSearch = () => {
    setSearchQuery({ keyword: '', tags: [] });
    setTagInput('');
  };

  return (
    <div className="max-w-4xl mx-auto space-y-6">
      {/* Header */}
      <div>
        <h1 className="text-3xl font-bold text-gray-900">Search Notes</h1>
        <p className="text-gray-600 mt-1">
          Find your notes by keyword or tags
        </p>
      </div>

      {/* Search Form */}
      <div className="card p-6 space-y-6">
        <form onSubmit={handleSearch} className="space-y-4">
          {/* Keyword Search */}
          <div>
            <label htmlFor="keyword" className="block text-sm font-medium text-gray-700 mb-2">
              Keyword
            </label>
            <div className="relative">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-gray-400" />
              <input
                type="text"
                id="keyword"
                value={searchQuery.keyword || ''}
                onChange={(e) => setSearchQuery({ ...searchQuery, keyword: e.target.value })}
                className="input pl-10"
                placeholder="Search in title and content..."
              />
            </div>
          </div>

          {/* Tag Selection */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Tags
            </label>
            
            {/* Selected Tags */}
            {searchQuery.tags && searchQuery.tags.length > 0 && (
              <div className="flex flex-wrap gap-2 mb-3">
                {searchQuery.tags.map((tag, index) => (
                  <span
                    key={index}
                    className="inline-flex items-center gap-1 px-3 py-1 rounded-full text-sm bg-primary-100 text-primary-800"
                  >
                    {tag}
                    <button
                      type="button"
                      onClick={() => removeTag(tag)}
                      className="hover:text-primary-600"
                    >
                      <X className="h-3 w-3" />
                    </button>
                  </span>
                ))}
              </div>
            )}

            {/* Available Tags */}
            {availableTags && availableTags.length > 0 && (
              <div>
                <p className="text-sm text-gray-600 mb-2">Available tags:</p>
                <div className="flex flex-wrap gap-2 mb-4">
                  {availableTags
                    .filter(tag => !searchQuery.tags?.includes(tag))
                    .map((tag, index) => (
                      <button
                        key={index}
                        type="button"
                        onClick={() => addTag(tag)}
                        className="inline-flex items-center px-3 py-1 rounded-full text-sm bg-gray-100 text-gray-700 hover:bg-gray-200 transition-colors"
                      >
                        <Tag className="h-3 w-3 mr-1" />
                        {tag}
                      </button>
                    ))}
                </div>
              </div>
            )}

            {/* Custom Tag Input */}
            <div className="flex space-x-2">
              <input
                type="text"
                value={tagInput}
                onChange={(e) => setTagInput(e.target.value)}
                onKeyPress={handleTagInputKeyPress}
                className="input flex-1"
                placeholder="Add custom tag..."
                maxLength={50}
              />
              <button
                type="button"
                onClick={addCustomTag}
                disabled={!tagInput.trim()}
                className="btn-secondary h-10 px-4"
              >
                Add
              </button>
            </div>
          </div>

          {/* Actions */}
          <div className="flex justify-between items-center pt-4 border-t">
            <button
              type="button"
              onClick={clearSearch}
              disabled={!hasActiveSearch}
              className="btn-ghost text-gray-600"
            >
              Clear Search
            </button>
            
            <div className="text-sm text-gray-600">
              {hasActiveSearch && (
                <>
                  {isLoading ? 'Searching...' : `${searchResults?.length || 0} results found`}
                </>
              )}
            </div>
          </div>
        </form>
      </div>

      {/* Search Results */}
      {hasActiveSearch && (
        <div>
          <h2 className="text-xl font-semibold text-gray-900 mb-4">
            Search Results
          </h2>
          
          {isLoading ? (
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
                    </div>
                  </div>
                </div>
              ))}
            </div>
          ) : error ? (
            <div className="card p-6 border-red-200 bg-red-50">
              <p className="text-sm text-red-600">
                {error.message || 'Failed to search notes. Please try again.'}
              </p>
            </div>
          ) : !searchResults || searchResults.length === 0 ? (
            <div className="text-center py-12">
              <Search className="h-12 w-12 text-gray-400 mx-auto mb-4" />
              <h3 className="text-lg font-semibold text-gray-900 mb-2">
                No notes found
              </h3>
              <p className="text-gray-600">
                Try adjusting your search criteria or browse all notes.
              </p>
            </div>
          ) : (
            <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
              {searchResults.map((note) => (
                <Link
                  key={note.id}
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
                      <FileText className="h-3 w-3 mr-1" />
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
              ))}
            </div>
          )}
        </div>
      )}

      {/* Initial State */}
      {!hasActiveSearch && (
        <div className="text-center py-12">
          <Search className="h-12 w-12 text-gray-400 mx-auto mb-4" />
          <h3 className="text-lg font-semibold text-gray-900 mb-2">
            Start searching
          </h3>
          <p className="text-gray-600">
            Enter a keyword or select tags to find your notes.
          </p>
        </div>
      )}
    </div>
  );
};

export default SearchPage;