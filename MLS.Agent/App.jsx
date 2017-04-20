import React, { Component } from 'react';
import MonacoEditor from 'react-monaco-editor';
import styles from "./style.css";
import parseQueryString from './client/source/utility.js';

class App extends Component {
  constructor(props) {
    super(props);
    
    let query = props.location && 
                props.location.search &&
                parseQueryString(props.location.search);

    this.state = {
      code: "// loading...",
      output: "Click run to get output",
      height: query['height'] || 140
    };
    
    if (!query || !this.tryLoadCodeFromUrl(query)) {
      this.loadDefault();
    }
    
    this.onClick = this.onClick.bind(this);
    this.onChange = this.onChange.bind(this);
  }
  
  tryLoadCodeFromUrl (query) {
   
    var from = query['from'];

    if (!from) {
      return false;
    }

    this.state = {
      code: `// loading from ${decodeURIComponent(from)} ...`,
      output: "Click run to get output",
      height: query['height'] || 140
    };

    this.loadCode(from)
        .then(r => {
          setTimeout(() => {
            this.setState({
              code: r, 
              loaded: true
            });
          }, 3000);
    });
    
    return true;
  }

  loadDefault() {
    setTimeout(() => {
         this.setState({
            code: 
`using System;

public class Program  
{ 
    public static void Main()
    {
        Console.WriteLine(\"Hello!\");  
    }
}`,
          });
      }, 500);
  }

  editorDidMount(editor, monaco) {
    editor.focus();
  }
  
  onChange(newValue, e) {
    console.log(e);
    this.setState({code: newValue});
  }
  
  onClick(e) {
    console.log(e);
    this.compileAndExecute(this.state.code)
    .then(result => {
      this.setState({output: result.output.map(l => <span>{l} <br /></span>)});
    });
  }
  
  compileAndExecute(code)
  {
    return fetch('/api/compile', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        Source: code 
      })
    })
    .then(r => r.json());
  }
  
  loadCode(uri) {
    return fetch(`/api/code?from=${uri}`, { method: 'GET' })
    .then(r => r.text());
  }
  
  render() {
    
    const code = this.state.code;
    const options = {
      selectOnLineNumbers: true
    }; 
    
    return (
    
    <div>
   
      <div className={styles.content}>
          <div className={styles.editor}>
            <MonacoEditor
            height={this.state.height}
            language="csharp"
            value={code}
            options={options}
            onChange={this.onChange}
            editorDidMount={this.editorDidMount}
            />

            
          <div className={styles.controls}>
            <button onClick={this.onClick}>
              Run
            </button>
          </div>
          </div>
          
      </div>
      
      <div className={styles.terminal} 
           style={{maxHeight: Math.floor(this.state.height/2)}}>
        {this.state.output}
      </div> 
    
    </div>
    );
  }
}

export default App;
