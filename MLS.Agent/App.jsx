import React, { Component } from 'react';
import MonacoEditor from 'react-monaco-editor';
import styles from "./style.css";

class App extends Component {
  constructor(props) {
    super(props);
    
    this.state = {
      code: "// loading...",
      output: "Click run to get output"
    };
    
    var code;
    
    if (!this.tryLoadCodeFromUrl(props)) {
      this.loadDefault();
    }
    
    this.onClick = this.onClick.bind(this);
    this.onChange = this.onChange.bind(this);
  }
  
  tryLoadCodeFromUrl (props) {
    if (!props.location || !props.location.search) {
      return false;
    }

    var from = props
          .location
          .search
          .slice(1)
          .split('&')
          .map(kv => kv.split('='))
          .reduce((hash, pair) => { 
            hash[pair[0]] = pair[1];
            return hash;
          }, {})['from'];

      if (!from) {
        return false;
      }

      this.state = {
        code: `// loading from ${decodeURIComponent(from)} ...`,
        output: "Click run to get output"
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
    
    <div className={styles.parent}>
    
      <div className={styles.editor}>
        <MonacoEditor
        width="500"
        height="180"
        language="csharp"
        value={code}
        options={options}
        onChange={this.onChange}
        editorDidMount={this.editorDidMount}
        />
      </div>
      
      <div className={styles.controls}>
        <button onClick={this.onClick}>
          Run
        </button>
      </div>
      
      <div className={styles.terminal}>
        {this.state.output}
      </div> 
    
    </div>
    );
  }
}


export default App;
