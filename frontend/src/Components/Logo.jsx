import './Logo.css'

function Logo(){
    return(
        <div className="logo">
            <div className="logo_picture_container"><img className='logo_picture' src='/monolith.png'></img></div>
            <div className="logo_text">monolith</div>
        </div>
    );
}

export default Logo