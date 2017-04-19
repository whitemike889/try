import React from 'react'
import ReactDOM from 'react-dom'
import {
  BrowserRouter as Router,
  Route
} from 'react-router-dom'
import { AppContainer } from 'react-hot-loader'
import 'react-hot-loader/patch';
import App from './App.jsx'


const Routes = (props) => (
  <Router {...props}>
    <Route path="/" component={App} />
  </Router>
);

const render = App => {
  ReactDOM.render(
    <AppContainer>
      <Routes>
        <App />
      </Routes>
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