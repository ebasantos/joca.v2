import React, { useState } from 'react';
import {
  Container,
  Box,
  Typography,
  Button,
  Paper,
  Alert,
  LinearProgress,
} from '@mui/material';
import { Upload as UploadIcon } from '@mui/icons-material';

const FileUpload = () => {
  const [file, setFile] = useState(null);
  const [uploading, setUploading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  const handleFileChange = (event) => {
    const selectedFile = event.target.files[0];
    if (selectedFile && selectedFile.type === 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet') {
      setFile(selectedFile);
      setError('');
    } else {
      setError('Por favor, selecione um arquivo Excel (.xlsx)');
      setFile(null);
    }
  };

  const handleUpload = async () => {
    if (!file) {
      setError('Por favor, selecione um arquivo');
      return;
    }

    if (file.size > 10 * 1024 * 1024) { // 10MB
      setError('O arquivo é muito grande. O tamanho máximo permitido é 10MB');
      return;
    }

    setUploading(true);
    setError('');
    setSuccess('');

    const formData = new FormData();
    formData.append('file', file);

    try {
      const response = await fetch('http://localhost:5000/api/leads/upload', {
        method: 'POST',
        body: formData,
        credentials: 'include',
        headers: {
          'Accept': 'application/json',
        },
      });

      if (!response.ok) {
        const errorData = await response.json().catch(() => ({}));
        throw new Error(errorData.message || `Erro ${response.status}: ${response.statusText}`);
      }

      const data = await response.json();
      setSuccess(`Upload concluído com sucesso! ${data.count} leads importados.`);
      setFile(null);
    } catch (err) {
      console.error('Erro no upload:', err);
      setError(err.message || 'Erro ao fazer upload do arquivo. Por favor, tente novamente.');
    } finally {
      setUploading(false);
    }
  };

  return (
    <Container maxWidth="md">
      <Box sx={{ mt: 4 }}>
        <Paper elevation={3} sx={{ p: 4 }}>
          <Typography variant="h5" component="h1" gutterBottom>
            Upload de Leads
          </Typography>
          
          {error && (
            <Alert severity="error" sx={{ mb: 2 }}>
              {error}
            </Alert>
          )}
          
          {success && (
            <Alert severity="success" sx={{ mb: 2 }}>
              {success}
            </Alert>
          )}

          <Box sx={{ my: 3 }}>
            <input
              accept=".xlsx"
              style={{ display: 'none' }}
              id="file-upload"
              type="file"
              onChange={handleFileChange}
            />
            <label htmlFor="file-upload">
              <Button
                variant="outlined"
                component="span"
                startIcon={<UploadIcon />}
                sx={{ mb: 2 }}
              >
                Selecionar Arquivo
              </Button>
            </label>
            
            {file && (
              <Typography variant="body1" sx={{ mb: 2 }}>
                Arquivo selecionado: {file.name}
              </Typography>
            )}

            {uploading && <LinearProgress sx={{ mb: 2 }} />}

            <Button
              variant="contained"
              onClick={handleUpload}
              disabled={!file || uploading}
              fullWidth
            >
              {uploading ? 'Enviando...' : 'Enviar Arquivo'}
            </Button>
          </Box>

          <Typography variant="body2" color="text.secondary">
            Formatos aceitos: .xlsx (Excel)
          </Typography>
        </Paper>
      </Box>
    </Container>
  );
};

export default FileUpload; 