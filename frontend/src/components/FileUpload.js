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
import TestConnection from './TestConnection';

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

  const parseExcelFile = async (file) => {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();

      reader.onload = (e) => {
        try {
          const data = e.target.result;
          const workbook = window.XLSX.read(data, { type: 'array' });
          const sheetName = workbook.SheetNames[0];
          const worksheet = workbook.Sheets[sheetName];
          const jsonData = window.XLSX.utils.sheet_to_json(worksheet);

          // Map the Excel columns to the expected format
          const contacts = jsonData.map(row => {
            // Extract phone number and ensure it's converted to a string
            const phoneRaw = row.Telefone || row.telefone || row.PHONE || row.Phone || row.PhoneNumber || '';
            // Ensure phoneNumber is always a string, even if it's a number in the Excel file
            const phoneNumber = String(phoneRaw);

            return {
              name: row.Nome || row.nome || row.NAME || row.Name || '',
              phoneNumber: phoneNumber,
              email: row.Email || row.email || row.EMAIL || '',
              company: row.Empresa || row.empresa || row.COMPANY || row.Company || '',
              notes: row.Notas || row.notas || row.NOTES || row.Notes || ''
            };
          }).filter(contact => contact.name && contact.phoneNumber);

          resolve(contacts);
        } catch (error) {
          reject(error);
        }
      };

      reader.onerror = (error) => {
        reject(error);
      };

      reader.readAsArrayBuffer(file);
    });
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

    try {
      // Parse the Excel file
      const contacts = await parseExcelFile(file);

      if (contacts.length === 0) {
        throw new Error('Não foram encontrados contatos válidos no arquivo');
      }

      // Send the parsed data to the backend
      console.log('Sending contacts:', contacts);
      // Add additional logging for phoneNumber types
      contacts.forEach((contact, index) => {
        console.log(`Contact ${index} phone type:`, typeof contact.phoneNumber, 'Value:', contact.phoneNumber);
      });

      const response = await fetch('http://localhost:5000/api/leads/upload', {
        method: 'POST',
        body: JSON.stringify({ Contacts: contacts }),
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json',
          ...localStorage.getItem('user') ? { 'Authorization': `Bearer ${JSON.parse(localStorage.getItem('user')).token}` } : {}
        },
      });

      // Log detailed response for debugging
      console.log('Response status:', response.status);
      console.log('Response headers:', Object.fromEntries([...response.headers.entries()]));

      if (!response.ok) {
        const errorData = await response.json().catch(() => ({}));
        throw new Error(errorData.message || `Erro ${response.status}: ${response.statusText}`);
      }

      const data = await response.json();
      setSuccess(
        `Upload concluído com sucesso! ${data.count} leads processados: ` +
        `${data.newContacts} novos contatos e ${data.updatedContacts} contatos atualizados.`
      );
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
        <TestConnection />

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