import React from 'react'
import ReactDOM from 'react-dom'
import { AppContainer } from 'react-hot-loader'
import 'react-hot-loader/patch';
import App from './App.jsx'

const render = App => {
  ReactDOM.render(
    <AppContainer>
      <App />
    </AppContainer>,
    document.getElementById('root')
  )
}

render(App)

if (module.hot) {
  module.hot.accept('./App.jsx', () => {
    const NextRootContainer = require('./App.jsx');
    render(NextRootContainer);
  });
}