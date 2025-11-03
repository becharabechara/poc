import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { ArrowLeft, Save, X, Plus, Sparkles, Hash } from 'lucide-react';
import { useCreateNote } from '../hooks/useNotes';
import { CreateNoteRequest } from '../types';

const CreateNotePage: React.FC = () => {
  const navigate = useNavigate();
  const createNoteMutation = useCreateNote();
  
  const [formData, setFormData] = useState<CreateNoteRequest>({
    title: '',
    content: '',
    tags: [],
  });
  
  const [tagInput, setTagInput] = useState('');
  const [errors, setErrors] = useState<Record<string, string>>({});

  const validateForm = (): boolean => {
    const newErrors: Record<string, string> = {};
    
    if (!formData.title.trim()) {
      newErrors.title = 'Title is required';
    } else if (formData.title.length > 200) {
      newErrors.title = 'Title must be less than 200 characters';
    }
    
    if (formData.content.length > 10000) {
      newErrors.content = 'Content must be less than 10,000 characters';
    }
    
    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!validateForm()) {
      return;
    }

    try {
      const newNote = await createNoteMutation.mutateAsync(formData);
      navigate(`/notes/${newNote.id}`);
    } catch (error) {
      console.error('Failed to create note:', error);
    }
  };

  const addTag = () => {
    const tag = tagInput.trim().toLowerCase();
    if (tag && !formData.tags?.includes(tag)) {
      setFormData({
        ...formData,
        tags: [...(formData.tags || []), tag],
      });
      setTagInput('');
    }
  };

  const removeTag = (tagToRemove: string) => {
    setFormData({
      ...formData,
      tags: formData.tags?.filter(tag => tag !== tagToRemove) || [],
    });
  };

  const handleTagInputKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter') {
      e.preventDefault();
      addTag();
    }
  };

  const suggestedTags = ['work', 'personal', 'idea', 'important', 'project', 'meeting', 'todo', 'inspiration'];

  return (
    <div className="max-w-4xl mx-auto">
      {/* Modern Header */}
      <div className="flex items-center justify-between mb-8">
        <div className="flex items-center space-x-4">
          <button
            onClick={() => navigate(-1)}
            className="btn-secondary-modern p-3"
          >
            <ArrowLeft className="w-5 h-5" />
          </button>
          <div>
            <h1 className="text-3xl font-bold text-gray-900">Create New Note</h1>
            <p className="text-gray-600 mt-1">Capture your thoughts and ideas</p>
          </div>
        </div>
        
        <div className="flex items-center space-x-3">
          <button
            type="button"
            onClick={() => navigate(-1)}
            className="btn-secondary-modern"
          >
            <X className="w-4 h-4" />
            Cancel
          </button>
          <button
            type="submit"
            form="note-form"
            disabled={createNoteMutation.isPending}
            className="btn-modern"
          >
            <Save className="w-4 h-4" />
            {createNoteMutation.isPending ? 'Saving...' : 'Save Note'}
          </button>
        </div>
      </div>

      {/* Modern Form */}
      <form id="note-form" onSubmit={handleSubmit} className="space-y-8">
        <div className="modern-form space-y-8">
          {/* Title Section */}
          <div className="form-group">
            <label htmlFor="title" className="form-label flex items-center">
              <Sparkles className="w-4 h-4 mr-2 text-blue-600" />
              Title *
            </label>
            <input
              type="text"
              id="title"
              value={formData.title}
              onChange={(e) => setFormData({ ...formData, title: e.target.value })}
              className={`modern-input ${errors.title ? 'border-red-400 focus:border-red-500' : ''}`}
              placeholder="Give your note a meaningful title..."
              required
            />
            {errors.title && (
              <p className="mt-2 text-sm text-red-600 flex items-center">
                <X className="w-4 h-4 mr-1" />
                {errors.title}
              </p>
            )}
          </div>

          {/* Content Section */}
          <div className="form-group">
            <label htmlFor="content" className="form-label">
              Content
            </label>
            <textarea
              id="content"
              rows={14}
              value={formData.content}
              onChange={(e) => setFormData({ ...formData, content: e.target.value })}
              className={`modern-textarea ${errors.content ? 'border-red-400 focus:border-red-500' : ''}`}
              placeholder="Start writing your note here... You can include ideas, observations, plans, or anything that matters to you."
            />
            {errors.content && (
              <p className="mt-2 text-sm text-red-600 flex items-center">
                <X className="w-4 h-4 mr-1" />
                {errors.content}
              </p>
            )}
            <div className="flex justify-between items-center mt-2">
              <p className="text-sm text-gray-500">
                {formData.content.length}/10,000 characters
              </p>
              <div className="flex items-center text-sm text-gray-500">
                <div className={`w-2 h-2 rounded-full mr-2 ${formData.content.length > 0 ? 'bg-green-400' : 'bg-gray-300'}`}></div>
                {formData.content.length > 0 ? 'Draft saved' : 'Start typing...'}
              </div>
            </div>
          </div>

          {/* Tags Section */}
          <div className="form-group">
            <label className="form-label flex items-center">
              <Hash className="w-4 h-4 mr-2 text-blue-600" />
              Tags
              <span className="text-sm font-normal text-gray-500 ml-2">Help organize your notes</span>
            </label>
            
            <div className="space-y-4">
              {/* Tag Input */}
              <div className="flex space-x-3">
                <div className="flex-1 relative">
                  <input
                    type="text"
                    value={tagInput}
                    onChange={(e) => setTagInput(e.target.value)}
                    onKeyPress={handleTagInputKeyPress}
                    className="modern-input pl-10"
                    placeholder="Add a tag..."
                    maxLength={50}
                  />
                  <Hash className="absolute left-3 top-1/2 transform -translate-y-1/2 w-4 h-4 text-gray-400" />
                </div>
                <button
                  type="button"
                  onClick={addTag}
                  disabled={!tagInput.trim()}
                  className="btn-modern"
                >
                  <Plus className="w-4 h-4" />
                  Add
                </button>
              </div>

              {/* Suggested Tags */}
              {(!formData.tags || formData.tags.length === 0) && (
                <div>
                  <p className="text-sm text-gray-600 mb-2">Quick suggestions:</p>
                  <div className="flex flex-wrap gap-2">
                    {suggestedTags.map((tag) => (
                      <button
                        key={tag}
                        type="button"
                        onClick={() => {
                          setTagInput(tag);
                          setTimeout(() => addTag(), 0);
                        }}
                        className="modern-tag hover:bg-blue-100 cursor-pointer transition-colors"
                      >
                        {tag}
                      </button>
                    ))}
                  </div>
                </div>
              )}
              
              {/* Current Tags */}
              {formData.tags && formData.tags.length > 0 && (
                <div>
                  <p className="text-sm text-gray-600 mb-3">Your tags:</p>
                  <div className="flex flex-wrap gap-3">
                    {formData.tags.map((tag, index) => (
                      <span
                        key={index}
                        className="inline-flex items-center gap-2 px-4 py-2 rounded-full text-sm bg-gradient-to-r from-blue-100 to-purple-100 text-blue-800 border border-blue-200"
                      >
                        <Hash className="w-3 h-3" />
                        {tag}
                        <button
                          type="button"
                          onClick={() => removeTag(tag)}
                          className="hover:text-red-600 transition-colors ml-1"
                        >
                          <X className="w-3 h-3" />
                        </button>
                      </span>
                    ))}
                  </div>
                </div>
              )}
            </div>
          </div>
        </div>

        {/* Error Display */}
        {createNoteMutation.error && (
          <div className="modern-card p-6 border-2 border-red-200 bg-red-50">
            <div className="flex items-center">
              <X className="w-5 h-5 text-red-600 mr-3" />
              <p className="text-red-700 font-medium">
                {createNoteMutation.error.message || 'Failed to create note. Please try again.'}
              </p>
            </div>
          </div>
        )}
      </form>
    </div>
  );
};

export default CreateNotePage;