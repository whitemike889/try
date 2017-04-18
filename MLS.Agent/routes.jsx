import React from 'react'

import {
  BrowserRouter as Router,
  Route
} from 'react-router-dom'

import App from './App.jsx';

const Routes = (props) => (
  <Router {...props}>
    <Route path="/" component={App} />
    {/*<Route path="/about" component={About} />
    <Route path="*" component={NotFound} />*/}
  </Router>
);

export default Routes;