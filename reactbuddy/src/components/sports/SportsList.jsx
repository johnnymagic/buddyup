import React, { useEffect } from 'react';
import { useSports } from '../context/SportsContext';
import { Table, Button, Badge, Spinner, Alert } from 'react-bootstrap';
import { FaEdit, FaToggleOn, FaToggleOff } from 'react-icons/fa';

const SportsList = () => {
  const { sports, loading, error, fetchSports, selectSport } = useSports();

  useEffect(() => {
    fetchSports();
  }, [fetchSports]);

  if (loading) {
    return (
      <div className="text-center py-5">
        <Spinner animation="border" role="status">
          <span className="visually-hidden">Loading...</span>
        </Spinner>
      </div>
    );
  }

  if (error) {
    return (
      <Alert variant="danger">
        Error loading sports: {error}
      </Alert>
    );
  }

  return (
    <div className="sports-list">
      <div className="d-flex justify-content-between align-items-center mb-4">
        <h2>Sports</h2>
        <Button variant="primary" onClick={() => selectSport(null)}>
          Add New Sport
        </Button>
      </div>

      <Table striped bordered hover responsive>
        <thead>
          <tr>
            <th>Name</th>
            <th>Description</th>
            <th>Icon</th>
            <th>Status</th>
            <th>Created By</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          {sports.length === 0 ? (
            <tr>
              <td colSpan="6" className="text-center">No sports found</td>
            </tr>
          ) : (
            sports.map(sport => (
              <tr key={sport.sportId}>
                <td>{sport.name}</td>
                <td>{sport.description || 'N/A'}</td>
                <td>
                  {sport.iconUrl ? (
                    <img 
                      src={sport.iconUrl} 
                      alt={sport.name} 
                      style={{ width: '32px', height: '32px' }} 
                    />
                  ) : (
                    'No icon'
                  )}
                </td>
                <td>
                  <Badge bg={sport.isActive ? 'success' : 'danger'}>
                    {sport.isActive ? 'Active' : 'Inactive'}
                  </Badge>
                </td>
                <td>{sport.createdBy || 'System'}</td>
                <td>
                  <Button 
                    variant="outline-primary" 
                    size="sm" 
                    className="me-2"
                    onClick={() => selectSport(sport)}
                  >
                    <FaEdit /> Edit
                  </Button>
                  <Button 
                    variant={sport.isActive ? 'outline-danger' : 'outline-success'} 
                    size="sm"
                  >
                    {sport.isActive ? <FaToggleOff /> : <FaToggleOn />}
                    {' '}
                    {sport.isActive ? 'Deactivate' : 'Activate'}
                  </Button>
                </td>
              </tr>
            ))
          )}
        </tbody>
      </Table>
    </div>
  );
};

export default SportsList;