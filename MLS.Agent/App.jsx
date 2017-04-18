import React, { Component } from 'react';
import MonacoEditor from 'react-monaco-editor';

class App extends Component {
  constructor(props) {
    super(props);
    this.state = {  
      code: [
        "using System;",
        "",
        "public class HelloWorld",
        "{", 
        "    public static void Main()",
        "    {",
        "        Console.WriteLine($\"Hello world! {DateTime.Now}!\");",
        "    }",
        "}",
        ].join('\n'),
      output: "Click run to get output"
    }

    this.onClick = this.onClick.bind(this);
    this.onChange = this.onChange.bind(this);
  }

  editorDidMount(editor, monaco) {
    editor.focus();
  }

  onChange(newValue, e) {
    this.setState({code: newValue});
  }

  onClick(e) {
    this.getOutput(this.state.code)
        .then(result => {
          this.setState({output: result.output.map(l => <p>{l}</p>)});
        });
  }

  getOutput(code)
  {
    return fetch('/api/compile', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        Source: code 
      })
    }).then(r => {
        return r.json();
    });
  }

  render() {
    const code = this.state.code;
    const options = {
      selectOnLineNumbers: true
    };
    return (
      <div>
        <div>
          <MonacoEditor
            width="800"
            height="600"
            language="csharp"
            value={code}
            options={options}
            onChange={this.onChange}
            editorDidMount={this.editorDidMount}
          />
        </div>
        <div className='mlsTerminal'>
          {this.state.output}
        </div>
        <div>
          <button onClick={this.onClick}>
            Run
          </button>
        </div>
      </div>
    );
  }
}

export default App;
