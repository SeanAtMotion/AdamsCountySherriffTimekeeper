import { createTheme } from '@mui/material/styles'

export const brand = {
  navy: '#002147',
  gold: '#C5A059',
  paleBlue: '#D1E3F8',
  pageBg: '#F0F4F8',
}

export const appTheme = createTheme({
  palette: {
    mode: 'light',
    primary: { main: brand.navy },
    secondary: { main: brand.gold },
    background: { default: brand.pageBg, paper: '#fff' },
  },
  typography: {
    fontFamily: '"Inter", "Segoe UI", "Roboto", "Helvetica Neue", Arial, sans-serif',
    h1: { fontFamily: '"Georgia", "Times New Roman", serif', fontWeight: 600 },
    h2: { fontFamily: '"Georgia", "Times New Roman", serif', fontWeight: 600 },
    h3: { fontFamily: '"Georgia", "Times New Roman", serif', fontWeight: 600 },
    h4: { fontFamily: '"Georgia", "Times New Roman", serif', fontWeight: 600 },
    h5: { fontFamily: '"Georgia", "Times New Roman", serif', fontWeight: 600 },
    h6: { fontFamily: '"Georgia", "Times New Roman", serif', fontWeight: 600 },
  },
  shape: { borderRadius: 4 },
  components: {
    MuiButton: {
      styleOverrides: {
        root: { textTransform: 'none', fontWeight: 600 },
      },
    },
  },
})
