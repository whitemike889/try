import React from 'react'
import ReactDOM from 'react-dom'

import { AppContainer } from 'react-hot-loader'
import 'react-hot-loader/patch';
import App from './App.jsx'

const render = App => {
  ReactDOM.render(
    <AppContainer>
      <App codeUrl={ 'https%3A%2F%2Fgist.githubusercontent.com%2Fjonsequitur%2Fb36ec7591de8f2fa58b99278953cd557%2Fraw%2F836753209d481333d9cf3d01d9897b1e1cad8eb6%2Fdeferred%252520execution%252520of%252520Select' } />
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