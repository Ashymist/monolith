import './Login.css'
import { useState } from "react";
import { useNavigate } from "react-router-dom";

function Login(){
const navigateToHome = useNavigate();
const [password, setPassword] = useState("");

const getCookie = async (event) => {
    event.preventDefault();
    const requestOptions = {
            method: 'POST',
            headers : {'Content-Type': 'application/json' },
            body: JSON.stringify({password: password})
        }

        const res = await fetch("http://localhost:5173/api/login", requestOptions);
        
        if(res.status == 401) alert("Invalid password");
        if(res.status == 200) navigateToHome('/home');
};

    return(
        <div className="login">
            <div className='monolith_title_card'>monolith</div>
            <div className='prompt'>Enter your vault</div>
            <form className='password_form' onSubmit={getCookie}>
                <input type='password' id='password_input' placeholder='Password' value={password} onChange={e => setPassword(e.target.value)}></input>
            </form>
            <div className='monolith'>
                <img src='/monolith.png' className='monolith_image'></img>
            </div>
            
        </div>
    );
}

export default Login