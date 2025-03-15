import React from 'react';
import { Container, Row, Col } from 'react-bootstrap';
import { SportsProvider } from '../context/SportsContext';
import SportsList from '../components/SportsList';
import SportForm from '../components/SportForm';

const SportsPage = () => {
  return (
    <SportsProvider>
      <Container fluid className="py-4">
        <Row className="mb-4">
          <Col>
            <h1>Sports Management</h1>
            <p className="text-muted">
              Manage sports available in the BuddyUp platform. Users can select these sports as their interests.
            </p>
          </Col>
        </Row>
        
        <Row>
          <Col lg={4} className="mb-4">
            <SportForm />
          </Col>
          <Col lg={8}>
            <SportsList />
          </Col>
        </Row>
      </Container>
    </SportsProvider>
  );
};

export default SportsPage;