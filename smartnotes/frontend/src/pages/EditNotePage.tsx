import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { ArrowLeft, Save, X, Edit3, Tag, Hash, Sparkles, FileText, AlertCircle } from 'lucide-react';
import { useNote, useUpdateNote, useTags } from '../hooks/useNotes';
import { UpdateNoteRequest } from '../types';

const EditNotePage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { data: note, isLoading: isLoadingNote } = useNote(id!);
  const { data: availableTags } = useTags();
  const updateNoteMutation = useUpdateNote();
  
  const [formData, setFormData] = useState<UpdateNoteRequest>({
    id: id!,
    title: '',
    content: '',
    tags: [],
  });
  
  const [tagInput, setTagInput] = useState('');
  const [errors, setErrors] = useState<Record<string, string>>({});

  useEffect(() => {
    if (note) {
      setFormData({
        id: note.id,
        title: note.title,
        content: note.content,
        tags: [...note.tags],
      });
    }
  }, [note]);

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
      await updateNoteMutation.mutateAsync({
        id: formData.id,
        data: formData,
      });
      navigate(`/notes/${formData.id}`);
    } catch (error) {
      console.error('Failed to update note:', error);
    }
  };

  const addTag = (tag: string) => {
    const cleanTag = tag.trim().toLowerCase();
    if (cleanTag && !formData.tags?.includes(cleanTag)) {
      setFormData({
        ...formData,
        tags: [...(formData.tags || []), cleanTag],
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
      addTag(tagInput);
    }
  };

  const getSuggestedTags = () => {
    if (!availableTags) return [];
    return availableTags
      .filter(tag => !formData.tags?.includes(tag))
      .filter(tag => !tagInput || tag.toLowerCase().includes(tagInput.toLowerCase()))
      .slice(0, 5);
  };

  if (isLoadingNote) {
    return (
      <div className="max-w-5xl mx-auto">
        <div className="space-y-8">
          {/* Header Skeleton */}
          <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-6">
            <div className="flex items-center space-x-4">
              <div className="loading-skeleton w-12 h-12 rounded-xl"></div>
              <div>
                <div className="loading-skeleton w-48 h-8 rounded-lg mb-2"></div>
                <div className="loading-skeleton w-32 h-5 rounded"></div>
              </div>
            </div>
            <div className="flex space-x-3">
              <div className="loading-skeleton w-24 h-12 rounded-xl"></div>
              <div className="loading-skeleton w-32 h-12 rounded-xl"></div>
            </div>
          </div>
          
          {/* Form Skeleton */}
          <div className="modern-form">
            <div className="space-y-8">
              <div className="loading-skeleton h-12 rounded-xl"></div>
              <div className="loading-skeleton h-64 rounded-xl"></div>
              <div className="loading-skeleton h-32 rounded-xl"></div>
            </div>
          </div>
        </div>
      </div>
    );
  }

  if (!note) {
    return (
      <div className="max-w-5xl mx-auto">
        <div className="empty-state">
          <div className="modern-card p-12">
            <div className="empty-state-icon bg-gradient-to-br from-red-100 to-orange-100 rounded-2xl flex items-center justify-center">
              <FileText className="w-8 h-8 text-red-600" />
            </div>
            <h3 className="empty-state-title">
              Note not found
            </h3>
            <p className="empty-state-description">
              The note you're trying to edit doesn't exist or has been deleted.
            </p>
            <button onClick={() => navigate('/')} className="btn-modern">
              <ArrowLeft className="w-4 h-4" />
              Back to Notes
            </button>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="max-w-5xl mx-auto space-y-8">
      {/* Modern Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-6">
        <div className="flex items-center space-x-4">
          <button
            onClick={() => navigate(-1)}
            className="btn-icon-modern"
            title="Go back"
          >
            <ArrowLeft className="w-5 h-5" />
          </button>
          <div>
            <h1 className="text-3xl font-bold text-gray-900 flex items-center">
              <Edit3 className="w-7 h-7 mr-3 text-blue-600" />
              Edit Note
            </h1>
            <p className="text-gray-600 mt-1">
              Update your note content and tags
            </p>
          </div>
        </div>
        
        <div className="flex items-center space-x-3">
          <button
            type="button"
            onClick={() => navigate(`/notes/${id}`)}
            className="btn-secondary-modern"
          >
            <X className="w-4 h-4" />
            Cancel
          </button>
          <button
            type="submit"
            form="note-form"
            disabled={updateNoteMutation.isPending}
            className="btn-modern"
          >
            <Save className="w-4 h-4" />
            {updateNoteMutation.isPending ? 'Saving...' : 'Save Changes'}
          </button>
        </div>
      </div>

      {/* Modern Form */}
      <form id="note-form" onSubmit={handleSubmit} className="modern-form space-y-8">
        {/* Title Field */}
        <div className="form-group">
          <label htmlFor="title" className="form-label flex items-center">
            <Sparkles className="w-4 h-4 mr-2 text-blue-600" />
            Note Title
            <span className="text-red-500 ml-1">*</span>
          </label>
          <input
            type="text"
            id="title"
            value={formData.title}
            onChange={(e) => setFormData({ ...formData, title: e.target.value })}
            className={`modern-input ${errors.title ? 'border-red-500 focus:ring-red-500' : ''}`}
            placeholder="Enter a compelling title for your note..."
            required
            maxLength={200}
          />
          {errors.title && (
            <div className="error-message">
              <AlertCircle className="w-4 h-4" />
              {errors.title}
            </div>
          )}
          <div className="form-hint">
            {formData.title.length}/200 characters
          </div>
        </div>

        {/* Content Field */}
        <div className="form-group">
          <label htmlFor="content" className="form-label flex items-center">
            <FileText className="w-4 h-4 mr-2 text-blue-600" />
            Content
          </label>
          <textarea
            id="content"
            rows={16}
            value={formData.content}
            onChange={(e) => setFormData({ ...formData, content: e.target.value })}
            className={`modern-textarea ${errors.content ? 'border-red-500 focus:ring-red-500' : ''}`}
            placeholder="Share your thoughts, ideas, or knowledge here..."
            maxLength={10000}
          />
          {errors.content && (
            <div className="error-message">
              <AlertCircle className="w-4 h-4" />
              {errors.content}
            </div>
          )}
          <div className="form-hint">
            {formData.content.length}/10,000 characters
          </div>
        </div>

        {/* Tags Field */}
        <div className="form-group">
          <label className="form-label flex items-center">
            <Hash className="w-4 h-4 mr-2 text-blue-600" />
            Tags
            {formData.tags && formData.tags.length > 0 && (
              <span className="ml-2 px-2 py-1 bg-blue-100 text-blue-800 rounded-full text-xs">
                {formData.tags.length} selected
              </span>
            )}
          </label>
          
          <div className="space-y-4">
            {/* Selected Tags */}
            {formData.tags && formData.tags.length > 0 && (
              <div>
                <p className="text-sm text-gray-600 mb-3">Current tags:</p>
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
                onClick={() => addTag(tagInput)}
                disabled={!tagInput.trim()}
                className="btn-modern"
              >
                <Tag className="w-4 h-4" />
                Add
              </button>
            </div>

            {/* Suggested Tags */}
            {getSuggestedTags().length > 0 && (
              <div>
                <p className="text-sm text-gray-600 mb-3">Suggested tags:</p>
                <div className="flex flex-wrap gap-2">
                  {getSuggestedTags().map((tag, index) => (
                    <button
                      key={index}
                      type="button"
                      onClick={() => addTag(tag)}
                      className="modern-tag hover:bg-blue-100 cursor-pointer transition-colors"
                    >
                      <Hash className="w-3 h-3 mr-1" />
                      {tag}
                    </button>
                  ))}
                </div>
              </div>
            )}
          </div>
        </div>

        {/* Form Actions */}
        <div className="flex flex-col sm:flex-row justify-between items-center gap-4 pt-6 border-t border-gray-200">
          <div className="text-sm text-gray-600">
            Last saved: Never
          </div>
          
          <div className="flex items-center space-x-3">
            <button
              type="button"
              onClick={() => navigate(`/notes/${id}`)}
              className="btn-secondary-modern"
            >
              <X className="w-4 h-4" />
              Cancel Changes
            </button>
            <button
              type="submit"
              disabled={updateNoteMutation.isPending}
              className="btn-modern"
            >
              <Save className="w-4 h-4" />
              {updateNoteMutation.isPending ? 'Saving...' : 'Save Changes'}
            </button>
          </div>
        </div>
      </form>

      {/* Error Display */}
      {updateNoteMutation.error && (
        <div className="modern-card p-6 border-2 border-red-200 bg-red-50">
          <div className="flex items-center">
            <AlertCircle className="w-5 h-5 text-red-600 mr-3" />
            <p className="text-red-700 font-medium">
              {updateNoteMutation.error.message || 'Failed to update note. Please try again.'}
            </p>
          </div>
        </div>
      )}
    </div>
  );
};

export default EditNotePage;