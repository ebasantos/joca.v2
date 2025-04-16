import React, { useState } from 'react';
import { Button, Box, Typography, Alert } from '@mui/material';

const TestConnection = () => {
    const [status, setStatus] = useState('');
    const [error, setError] = useState('');
    const [loading, setLoading] = useState(false);

    const testConnection = async () => {
        setLoading(true);
        setStatus('');
        setError('');

        try {
            // Test the ping endpoint
            const response = await fetch('http://localhost:5000/api/ping');

            // Log detailed response
            console.log('Ping response status:', response.status);

            if (!response.ok) {
                throw new Error(`Error ${response.status}: ${response.statusText}`);
            }

            const data = await response.json();
            setStatus(`Connection successful! Response: ${JSON.stringify(data)}`);
        } catch (err) {
            console.error('Connection test error:', err);
            setError(`Failed to connect: ${err.message}`);
        } finally {
            setLoading(false);
        }
    };

    return (
        <Box sx={{ mt: 2, mb: 4, p: 2, border: '1px dashed #ccc', borderRadius: 2 }}>
            <Typography variant="h6" gutterBottom>
                API Connection Test
            </Typography>

            <Button
                variant="outlined"
                onClick={testConnection}
                disabled={loading}
                sx={{ mb: 2 }}
            >
                {loading ? 'Testing...' : 'Test API Connection'}
            </Button>

            {status && (
                <Alert severity="success" sx={{ mb: 2 }}>
                    {status}
                </Alert>
            )}

            {error && (
                <Alert severity="error" sx={{ mb: 2 }}>
                    {error}
                </Alert>
            )}
        </Box>
    );
};

export default TestConnection; 