import React, { useState, useEffect } from 'react';
import {
  Container,
  Grid,
  Paper,
  Typography,
  Box,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  CircularProgress,
  Alert,
} from '@mui/material';
import {
  People as PeopleIcon,
  CheckCircle as CheckCircleIcon,
  Error as ErrorIcon,
  Pending as PendingIcon,
} from '@mui/icons-material';

const Dashboard = () => {
  const [stats, setStats] = useState({
    total: 0,
    processed: 0,
    pending: 0,
    failed: 0,
  });
  const [recentLeads, setRecentLeads] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    const fetchData = async () => {
      try {
        const [statsResponse, leadsResponse] = await Promise.all([
          fetch('http://localhost:5000/api/leads/stats'),
          fetch('http://localhost:5000/api/leads/recent'),
        ]);

        if (!statsResponse.ok || !leadsResponse.ok) {
          throw new Error('Erro ao carregar dados');
        }

        const statsData = await statsResponse.json();
        const leadsData = await leadsResponse.json();

        setStats(statsData);
        setRecentLeads(leadsData);
      } catch (err) {
        setError('Erro ao carregar dados do dashboard');
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, []);

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="60vh">
        <CircularProgress />
      </Box>
    );
  }

  if (error) {
    return (
      <Container maxWidth="lg" sx={{ mt: 4 }}>
        <Alert severity="error">{error}</Alert>
      </Container>
    );
  }

  return (
    <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
      <Grid container spacing={3}>
        {/* Cards de Estatísticas */}
        <Grid item xs={12} sm={6} md={3}>
          <Paper
            sx={{
              p: 2,
              display: 'flex',
              flexDirection: 'column',
              alignItems: 'center',
              bgcolor: '#e3f2fd',
            }}
          >
            <PeopleIcon sx={{ fontSize: 40, color: '#1976d2', mb: 1 }} />
            <Typography component="h2" variant="h6" color="primary">
              Total de Leads
            </Typography>
            <Typography component="p" variant="h4">
              {stats.total}
            </Typography>
          </Paper>
        </Grid>

        <Grid item xs={12} sm={6} md={3}>
          <Paper
            sx={{
              p: 2,
              display: 'flex',
              flexDirection: 'column',
              alignItems: 'center',
              bgcolor: '#e8f5e9',
            }}
          >
            <CheckCircleIcon sx={{ fontSize: 40, color: '#2e7d32', mb: 1 }} />
            <Typography component="h2" variant="h6" color="success.main">
              Processados
            </Typography>
            <Typography component="p" variant="h4">
              {stats.processed}
            </Typography>
          </Paper>
        </Grid>

        <Grid item xs={12} sm={6} md={3}>
          <Paper
            sx={{
              p: 2,
              display: 'flex',
              flexDirection: 'column',
              alignItems: 'center',
              bgcolor: '#fff3e0',
            }}
          >
            <PendingIcon sx={{ fontSize: 40, color: '#ed6c02', mb: 1 }} />
            <Typography component="h2" variant="h6" color="warning.main">
              Pendentes
            </Typography>
            <Typography component="p" variant="h4">
              {stats.pending}
            </Typography>
          </Paper>
        </Grid>

        <Grid item xs={12} sm={6} md={3}>
          <Paper
            sx={{
              p: 2,
              display: 'flex',
              flexDirection: 'column',
              alignItems: 'center',
              bgcolor: '#fbe9e7',
            }}
          >
            <ErrorIcon sx={{ fontSize: 40, color: '#d32f2f', mb: 1 }} />
            <Typography component="h2" variant="h6" color="error.main">
              Falhas
            </Typography>
            <Typography component="p" variant="h4">
              {stats.failed}
            </Typography>
          </Paper>
        </Grid>

        {/* Tabela de Leads Recentes */}
        <Grid item xs={12}>
          <Paper sx={{ p: 2 }}>
            <Typography component="h2" variant="h6" color="primary" gutterBottom>
              Leads Recentes
            </Typography>
            <TableContainer>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>Nome</TableCell>
                    <TableCell>Telefone</TableCell>
                    <TableCell>Status</TableCell>
                    <TableCell>Data de Importação</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {recentLeads.map((lead) => (
                    <TableRow key={lead.id}>
                      <TableCell>{lead.name}</TableCell>
                      <TableCell>{lead.phone}</TableCell>
                      <TableCell>
                        <Typography
                          variant="body2"
                          sx={{
                            color:
                              lead.status === 'processed'
                                ? 'success.main'
                                : lead.status === 'pending'
                                ? 'warning.main'
                                : 'error.main',
                          }}
                        >
                          {lead.status === 'processed'
                            ? 'Processado'
                            : lead.status === 'pending'
                            ? 'Pendente'
                            : 'Falha'}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        {new Date(lead.importDate).toLocaleDateString()}
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          </Paper>
        </Grid>
      </Grid>
    </Container>
  );
};

export default Dashboard; 