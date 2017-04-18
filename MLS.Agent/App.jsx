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

    if (props.codeUrl) {
      this.loadCode(props.codeUrl)
          .then(r => this.setState({
            code: r
          }));
    } else { 
      code = setState({
        code: [
          ` // ${JSON.stringify(props)} `,
          "using System;",
          "using System.Collections.Generic;",
          "",
          "public class Program",
          "{", 
          "    public static void Main()",
          "    {",
          "        var names = new List<string>",
          "        {",
          "            \"Alice\",",
          "            \"Bobby\",",
          "            \"Carol\",",
          "            \"Diane\"",
          "        };",
          "",
          "        names.ForEach(name => Console.WriteLine(name));",  
          "    }",
          "}",
        ].join('\n')
        });
    }

    this.onClick = this.onClick.bind(this);
    this.onChange = this.onChange.bind(this);
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
            width="650"
            height="350"
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
