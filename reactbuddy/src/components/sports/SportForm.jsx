import React, { useState, useEffect } from 'react';
import { Form, Button, Card, Alert } from 'react-bootstrap';
import { useSports } from '../context/SportsContext';

const SportForm = () => {
  const { selectedSport, addSport, updateSport, selectSport } = useSports();
  
  const [formData, setFormData] = useState({
    name: '',
    description: '',
    iconUrl: '',
    isActive: true
  });
  
  const [errors, setErrors] = useState({});
  const [submitting, setSubmitting] = useState(false);
  const [apiError, setApiError] = useState(null);
  const [successMessage, setSuccessMessage] = useState(null);
  
  // Reset form when selected sport changes
  useEffect(() => {
    if (selectedSport) {
      setFormData({
        name: selectedSport.name || '',
        description: selectedSport.description || '',
        iconUrl: selectedSport.iconUrl || '',
        isActive: selectedSport.isActive
      });
    } else {
      setFormData({
        name: '',
        description: '',
        iconUrl: '',
        isActive: true
      });
    }
    
    setErrors({});
    setApiError(null);
    setSuccessMessage(null);
  }, [selectedSport]);
  
  const validateForm = () => {
    const newErrors = {};
    
    if (!formData.name.trim()) {
      newErrors.name = 'Sport name is required';
    } else if (formData.name.length > 50) {
      newErrors.name = 'Sport name must be less than 50 characters';
    }
    
    if (formData.description && formData.description.length > 500) {
      newErrors.description = 'Description must be less than 500 characters';
    }
    
    if (formData.iconUrl && !isValidUrl(formData.iconUrl)) {
      newErrors.iconUrl = 'Please enter a valid URL';
    }
    
    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };
  
  const isValidUrl = (string) => {
    try {
      new URL(string);
      return true;
    } catch (_) {
      return false;
    }
  };
  
  const handleChange = (e) => {
    const { name, value, type, checked } = e.target;
    setFormData({
      ...formData,
      [name]: type === 'checkbox' ? checked : value
    });
    
    // Clear specific error when field is changed
    if (errors[name]) {
      setErrors({
        ...errors,
        [name]: undefined
      });
    }
    
    // Clear success/error messages when form is modified
    setSuccessMessage(null);
    setApiError(null);
  };
  
  const handleSubmit = async (e) => {
    e.preventDefault();
    
    if (!validateForm()) {
      return;
    }
    
    setSubmitting(true);
    setApiError(null);
    
    try {
      if (selectedSport) {
        // Update existing sport
        await updateSport(selectedSport.sportId, formData);
        setSuccessMessage(`Sport "${formData.name}" has been updated successfully.`);
      } else {
        // Create new sport
        await addSport(formData);
        setSuccessMessage(`Sport "${formData.name}" has been added successfully.`);
        // Reset form after successful creation
        setFormData({
          name: '',
          description: '',
          iconUrl: '',
          isActive: true
        });
      }
    } catch (error) {
      setApiError(error.response?.data?.message || error.message || 'An error occurred');
    } finally {
      setSubmitting(false);
    }
  };
  
  const handleCancel = () => {
    selectSport(null);
  };
  
  return (
    <Card className="mb-4">
      <Card.Header as="h5">
        {selectedSport ? `Edit Sport: ${selectedSport.name}` : 'Add New Sport'}
      </Card.Header>
      <Card.Body>
        {successMessage && (
          <Alert variant="success" dismissible onClose={() => setSuccessMessage(null)}>
            {successMessage}
          </Alert>
        )}
        
        {apiError && (
          <Alert variant="danger" dismissible onClose={() => setApiError(null)}>
            {apiError}
          </Alert>
        )}
        
        <Form onSubmit={handleSubmit}>
          <Form.Group className="mb-3" controlId="sportName">
            <Form.Label>Sport Name *</Form.Label>
            <Form.Control
              type="text"
              name="name"
              value={formData.name}
              onChange={handleChange}
              isInvalid={!!errors.name}
              placeholder="Enter sport name"
              required
            />
            <Form.Control.Feedback type="invalid">
              {errors.name}
            </Form.Control.Feedback>
          </Form.Group>
          
          <Form.Group className="mb-3" controlId="sportDescription">
            <Form.Label>Description</Form.Label>
            <Form.Control
              as="textarea"
              name="description"
              value={formData.description}
              onChange={handleChange}
              isInvalid={!!errors.description}
              placeholder="Enter sport description"
              rows={3}
            />
            <Form.Control.Feedback type="invalid">
              {errors.description}
            </Form.Control.Feedback>
          </Form.Group>
          
          <Form.Group className="mb-3" controlId="sportIconUrl">
            <Form.Label>Icon URL</Form.Label>
            <Form.Control
              type="text"
              name="iconUrl"
              value={formData.iconUrl}
              onChange={handleChange}
              isInvalid={!!errors.iconUrl}
              placeholder="Enter icon URL"
            />
            <Form.Control.Feedback type="invalid">
              {errors.iconUrl}
            </Form.Control.Feedback>
            <Form.Text className="text-muted">
              URL to an image that represents this sport
            </Form.Text>
          </Form.Group>
          
          {selectedSport && (
            <Form.Group className="mb-3" controlId="sportIsActive">
              <Form.Check
                type="checkbox"
                name="isActive"
                label="Active"
                checked={formData.isActive}
                onChange={handleChange}
              />
              <Form.Text className="text-muted">
                Inactive sports won't be shown to users
              </Form.Text>
            </Form.Group>
          )}
          
          <div className="d-flex justify-content-end">
            {selectedSport && (
              <Button 
                variant="outline-secondary" 
                onClick={handleCancel}
                className="me-2"
                disabled={submitting}
              >
                Cancel
              </Button>
            )}
            <Button 
              variant="primary" 
              type="submit"
              disabled={submitting}
            >
              {submitting ? 'Saving...' : (selectedSport ? 'Update Sport' : 'Add Sport')}
            </Button>
          </div>
        </Form>
      </Card.Body>
    </Card>
  );
};

export default SportForm;