var Pool = {
    showDebug: false,
    RED: 0,
    YELLOW: 1,
    WHITE: 2,
    BLACK: 3,
};








Pool.Preloader = function () { };

Pool.Preloader.prototype = {

    init: function () {

        this.input.maxPointers = 1;

        this.scale.pageAlignHorizontally = true;

        this.game.renderer.renderSession.roundPixels = true;

        this.physics.startSystem(Phaser.Physics.P2JS);

    },

    preload: function () {

        this.load.path = '/assets/';

        this.load.bitmapFont('fat-and-tiny');

        this.load.images([ 'logo', 'table', 'cushions', 'cue', 'fill' ]);

        this.load.spritesheet('balls', 'balls.png', 26, 26);

        this.load.physics('table');

    },

    create: function () {

        this.state.start('Pool.MainMenu');

    }

};

Pool.MainMenu = function () {
    this.stateText = "";
    this.startText = null;
    this.playerList = null;
};

Pool.MainMenu.prototype = {

    create: function () {

        this.stage.backgroundColor = 0x001b07;

        var logo = this.add.image(this.world.centerX, 140, 'logo');
        logo.anchor.x = 0.5;

        this.stateText = "Waiting the Other Player";
        this.startText = this.add.bitmapText(this.world.centerX, 460, 'fat-and-tiny', this.stateText, 64);
        this.startText.anchor.x = 0.5;
        this.startText.smoothed = false;
        this.startText.tint = 0xff0000;
        

        var that = this;
        var wsImpl = window.WebSocket || window.MozWebSocket;
        var personCount = document.getElementById('personCount');
        window.ws = new wsImpl('ws://127.0.0.1:7181/');
        console.log(window.ws);
        ws.onmessage = function (evt) {
            var object = JSON.parse(evt.data);
            switch (object.action) {
                case "prepare":
                    that.updateText(object.data);
                    personCount.innerHTML = object.data;
                    break;
                case "ready":
                    that.isReadyToStart(object);
                    break;
            }
        };
       
        // when the connection is established, this method is called
        ws.onopen = function () {

        };
        // when the connection is closed, this method is called
        ws.onclose = function () {
            personCount.innerHTML = "the server is not ready";
        }
    },
    //update the tips text
    updateText: function (data) {
        if (data === 2) {
            this.stateText = "Wait Player1 to begin";
            this.input.onDown.addOnce(this.readyRequest, this);
        } else {
            this.stateText = "Wait the Other Player";
        }
        this.startText.setText(this.stateText);
    },
    isReadyToStart:function(object) {
        if (object.isReady) {
            this.start(object.idArray);
        }
    },
    readyRequest:function() {
        ws.send("ready");
    },
    start: function (idArray) {
        this.state.start('Pool.Game');
    }

};

Pool.Game = function (game) {

    //this.player1 = null;
    //this.player2 = null;
    //this.currentPlayer = null;

    this.score1 = 0;
    this.scoreText1 = null;

    this.score2 = 0;
    this.scoreText2 = null;

    this.currentText = null;

    this.speed = 0;
    this.allowShotSpeed = 20.0;

    this.balls = null;
    this.shadows = null;

    this.cue = null;
    this.fill = null;
    this.fillRect = null;
    this.aimLine = null;

    this.cueball = null;

    this.resetting = false;
    this.placeball = null;
    this.placeballShadow = null;
    this.placeRect = null;

    this.pauseKey = null;
    this.debugKey = null;
    //0 means can't move, 1 means can move
    this.canMove = 0;
};

Pool.Game.prototype = {

    init: function (object) {
        this.score1 = 0;
        this.score2 = 0;
        this.speed = 0;
        this.resetting = false;
        //this.player1 = object.player1;
        //this.player2 = object.player2;
        //this.currentPlayer = this.player1;// player1 plays first
        //console.log(this.player1, this.player2);
    },

    create: function () {

        ws.send("start");
        this.stage.backgroundColor = 0x001b07;

        //  The table
        this.table = this.add.sprite(400, 300, 'table');

        this.physics.p2.enable(this.table, Pool.showDebug);

        this.table.body.static = true;
        this.table.body.clearShapes();
        this.table.body.loadPolygon('table', 'pool-table-physics-shape');

        this.tableMaterial = this.physics.p2.createMaterial('tableMaterial', this.table.body);

        //  The pockets
        this.pockets = this.add.sprite();

        this.physics.p2.enable(this.pockets, Pool.showDebug);

        this.pockets.body.static = true;

        this.pockets.body.clearShapes();

        this.pockets.body.addCircle(32, 64, 80);
        this.pockets.body.addCircle(16, 400, 80);
        this.pockets.body.addCircle(32, 736, 80);

        this.pockets.body.addCircle(32, 64, 528);
        this.pockets.body.addCircle(16, 400, 528);
        this.pockets.body.addCircle(32, 736, 528);

        //  Ball shadows
        this.shadows = this.add.group();

        //  The cushions sit above the shadow layer
        this.add.sprite(0, 0, 'cushions');

        //  The balls

        this.balls = this.add.physicsGroup(Phaser.Physics.P2JS);
        this.balls.enableBodyDebug = Pool.showDebug;

        this.ballMaterial = this.physics.p2.createMaterial('ballMaterial');

        //  Row 1 (5 balls)

        var y = 241;

        this.makeBall(200, y, Pool.RED);
        this.makeBall(200, y + 32, Pool.YELLOW);
        this.makeBall(200, y + 64, Pool.YELLOW);
        this.makeBall(200, y + 96, Pool.RED);
        this.makeBall(200, y + 128, Pool.YELLOW);

        //  Row 2 (4 balls)

        y = 257;

        this.makeBall(232, y, Pool.YELLOW);
        this.makeBall(232, y + 32, Pool.RED);
        this.makeBall(232, y + 64, Pool.YELLOW);
        this.makeBall(232, y + 96, Pool.RED);

        //  Row 3 (3 balls including black)

        y = 273;

        this.makeBall(264, y, Pool.RED);
        this.makeBall(264, y + 32, Pool.BLACK);
        this.makeBall(264, y + 64, Pool.YELLOW);

        //  Row 4 (2 balls)

        y = 289;

        this.makeBall(296, y, Pool.YELLOW);
        this.makeBall(296, y + 32, Pool.RED);

        //  Row 5 (single red ball)

        this.makeBall(328, 305, Pool.RED);

        //  The cue ball

        this.cueball = this.makeBall(576, 305, Pool.WHITE);

        //  Our placing cue ball and its shadow
        this.placeball = this.add.sprite(0, 0, 'balls', Pool.WHITE);
        this.placeball.anchor.set(0.5);
        this.placeball.visible = false;

        this.placeballShadow = this.shadows.create(0, 0, 'balls', 4);
        this.placeballShadow.anchor.set(0.5);
        this.placeballShadow.visible = false;

        this.placeRect = new Phaser.Rectangle(112, 128, 576, 352);

        //  P2 Impact Events

        this.physics.p2.setImpactEvents(true);

        var ballVsTableMaterial = this.physics.p2.createContactMaterial(
            this.ballMaterial, this.tableMaterial);

        ballVsTableMaterial.restitution = 0.6;

        var ballVsBallMaterial = this.physics.p2.createContactMaterial(
            this.ballMaterial, this.ballMaterial);

        ballVsBallMaterial.restitution = 0.9;

        //  The cue
        this.cue = this.add.sprite(0, 0, 'cue');
        this.cue.anchor.y = 0.5;

        this.fill = this.add.sprite(0, 0, 'fill');
        this.fill.anchor.y = 0.5;
        this.fillRect = new Phaser.Rectangle(0, 0, 332, 6);
        this.fill.crop(this.fillRect);

        this.aimLine = new Phaser.Line(this.cueball.x, this.cueball.y, this.cueball.x, this.cueball.y);

        //  Score

        this.scoreText1 = this.add.bitmapText(16, 0, 'fat-and-tiny', 'PLAYER 1 SCORE: 0', 32);
        this.scoreText1.smoothed = false;

        this.scoreText2 = this.add.bitmapText(500, 0, 'fat-and-tiny', 'PLAYER 2 SCORE: 0', 32);
        this.scoreText2.smoothed = false;

        this.currentText = this.add.bitmapText(280, 40, 'fat-and-tiny', '1', 32);
        this.currentText.tint = 0xff0000;
        this.currentText.smoothed = false;
  
        //  Press P to pause and resume the game
        this.pauseKey = this.input.keyboard.addKey(Phaser.Keyboard.P);
        this.pauseKey.onDown.add(this.togglePause, this);

        //  Press D to toggle the debug display
        this.debugKey = this.input.keyboard.addKey(Phaser.Keyboard.D);
        this.debugKey.onDown.add(this.toggleDebug, this);

        //this.input.addMoveCallback(this.updateCue, this);
        //this.input.onDown.add(this.takeShot, this);


        var that = this;
        ws.onmessage = function (evt) {
            var object = JSON.parse(evt.data);

            switch (object.action) {
                case "updateMsg":
                    that.handleMove(object.code);
                    that.updateMsg(object);
                    break;
            }
        };
    },

    switchPlayer:function() {
        if (this.currentPlayer === this.player1) {
            this.currentPlayer = this.player2;
            this.currentText.setText("Now Player2 Move");
        }else{
            this.currentPlayer = this.player1;
            this.currentText.setText("Now Player1 Move");
        }
    },
    handleMove: function (code) {
        console.log(code)
        if (code === 0) {
            this.cue.visible = false;
            this.fill.visible = false;
            
        } else {
            this.cue.visible = true;
            this.fill.visible = true;

        }
    },
    updateMsg: function (object) {
        
        this.currentText.setText(object.msg + object.code);
    },

    togglePause: function () {

        this.game.paused = (this.game.paused) ? false : true;

    },

    toggleDebug: function () {

        Pool.showDebug = (Pool.showDebug) ? false : true;

        this.state.restart();

    },

    makeBall: function (x, y, color) {

        var ball = this.balls.create(x, y, 'balls', color);

        ball.body.setCircle(13);
        ball.body.fixedRotation = true;
        ball.body.setMaterial(this.ballMaterial);
        ball.body.damping = 0.40;
        ball.body.angularDamping = 0.45;
        ball.body.createBodyCallback(this.pockets, this.hitPocket, this);

        //  Link the two sprites together
        var shadow = this.shadows.create(x + 4, y + 4, 'balls', 4);
        shadow.anchor.set(0.5);

        ball.shadow = shadow;

        return ball;

    },

    takeShot: function () {

        if (this.speed > this.allowShotSpeed)
        {
            return;
        }

        var speed = (this.aimLine.length / 3);

        if (speed > 112)
        {
            speed = 112;
        }

        this.updateCue();

        var px = (Math.cos(this.aimLine.angle) * speed);
        var py = (Math.sin(this.aimLine.angle) * speed);

        this.cueball.body.applyImpulse([ px, py ], this.cueball.x, this.cueball.y);

        this.cue.visible = false;
        this.fill.visible = false;
        ws.send(1);
    },

    hitPocket: function (ball, pocket) {

        //  Cue ball reset
        if (ball.sprite === this.cueball)
        {
            this.resetCueBall();
        }
        else
        {
            ball.sprite.shadow.destroy();
            ball.sprite.destroy();

            this.score += 100;
            this.scoreText.text = "SCORE: " + this.score;

            if (this.balls.total === 1)
            {
                this.time.events.add(3000, this.gameOver, this);
            }

        }

    },

    resetCueBall: function () {

        this.cueball.body.setZeroVelocity();

        //  Move it to a 'safe' area
        this.cueball.body.x = 16;
        this.cueball.body.y = 16;

        this.resetting = true;

        //  We disable the physics body and stick the ball to the pointer
        this.cueball.visible = false;
        this.cueball.shadow.visible = false;

        this.placeball.x = this.input.activePointer.x;
        this.placeball.y = this.input.activePointer.y;
        this.placeball.visible = true;

        this.placeballShadow.x = this.placeball.x + 10;
        this.placeballShadow.y = this.placeball.y + 10;
        this.placeballShadow.visible = true;

        this.input.onDown.remove(this.takeShot, this);
        this.input.onDown.add(this.placeCueBall, this);

    },

    placeCueBall: function () {

        //  Check it's not colliding with other balls

        var a = new Phaser.Circle(this.placeball.x, this.placeball.y, 26);
        var b = new Phaser.Circle(0, 0, 26);

        for (var i = 0; i < this.balls.length; i++)
        {
            var ball = this.balls.children[i];

            if (ball.frame !== 2 && ball.exists)
            {
                b.x = ball.x;
                b.y = ball.y;

                if (Phaser.Circle.intersects(a, b))
                {
                    //  No point going any further
                    return;
                }
            }
        }

        this.cueball.reset(this.placeball.x, this.placeball.y);
        this.cueball.body.reset(this.placeball.x, this.placeball.y);
        this.cueball.visible = true;
        this.cueball.shadow.visible = true;

        this.placeball.visible = false;
        this.placeballShadow.visible = false;

        this.resetting = false;

        this.input.onDown.remove(this.placeCueBall, this);
        this.input.onDown.add(this.takeShot, this);

    },

    updateCue: function () {

        this.aimLine.start.set(this.cueball.x, this.cueball.y);
        this.aimLine.end.set(this.input.activePointer.x, this.input.activePointer.y);

        this.cue.position.copyFrom(this.aimLine.start);
        this.cue.rotation = this.aimLine.angle;

        this.fill.position.copyFrom(this.aimLine.start);
        this.fill.rotation = this.aimLine.angle;

        this.fillRect.width = this.aimLine.length;
        this.fill.updateCrop();

    },

    update: function () {

        if (this.resetting)
        {
            this.placeball.x = this.math.clamp(this.input.x, this.placeRect.left, this.placeRect.right);
            this.placeball.y = this.math.clamp(this.input.y, this.placeRect.top, this.placeRect.bottom);
            this.placeballShadow.x = this.placeball.x + 10;
            this.placeballShadow.y = this.placeball.y + 10;
        }
        else
        {
            this.updateSpeed();
            this.updateCue();
        }

    },

    updateSpeed: function () {

        this.speed = Math.sqrt(this.cueball.body.velocity.x * this.cueball.body.velocity.x + this.cueball.body.velocity.y * this.cueball.body.velocity.y);

        if (this.speed < this.allowShotSpeed)
        {
            if (!this.cue.visible)
            {
                this.cue.visible = true;
                this.fill.visible = true;
            }
        }
        else if (this.speed < 3.0)
        {
            this.cueball.body.setZeroVelocity();
        }

    },

    preRender: function () {

        this.balls.forEach(this.positionShadow, this);

    },

    positionShadow: function (ball) {

        ball.shadow.x = ball.x + 4;
        ball.shadow.y = ball.y + 4;

    },

    gameOver: function () {

        this.state.start('Pool.MainMenu');

    },

    render: function () {

        if (Pool.showDebug)
        {
            if (this.speed < 6)
            {
                this.game.debug.geom(this.aimLine);
            }

            this.game.debug.text("speed: " + this.speed, 540, 24);
            this.game.debug.text("power: " + (this.aimLine.length / 3), 540, 48);
        }

    }

};

var game = new Phaser.Game(800, 600, Phaser.CANVAS, 'game', null, false, true);

game.state.add('Pool.Preloader', Pool.Preloader);
game.state.add('Pool.MainMenu', Pool.MainMenu);
game.state.add('Pool.Game', Pool.Game);

game.state.start('Pool.Preloader');

