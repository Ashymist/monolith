import './Logout.css'
import { useNavigate } from "react-router-dom";
function Logout(){

    const navigateToLogin = useNavigate();

    const SignOutUser = async () => {
        const requestOptions = {
                method: 'POST',
            }
        const res = await fetch("http://localhost:5173/api/logout", requestOptions);

        if(res.status == 200) navigateToLogin('/login');
    }

    return(<div className="logout"onClick={SignOutUser}>Log out</div>)
}


export default Logout