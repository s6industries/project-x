# weapons - sheild, bomb, boomerang, traps bomb, snowball, mjolnir, tredent, shuriken
import pygame
import math
import json
import random

class Weapon:
    
    def __init__(self, player, screen, scale, name, animated=False, _type='weapon'):
        self.player = player
        self.screen = screen
        self.scale = scale
        self.name = name
        self.animated = animated
        
        self._type = _type
        self.data = None
        self.code = None

        # keys
        self.triggered = False
        self.c_triggered = False
        self.level = 1
        self.effect = False

        self.damage = 0.5 # default damage
        self.speed = 5 # default speed
        self.absorb = 0 # sheild absorb
        self.mana = 0.2
        self.slow = 0
        self.score = 100

        self.weaponData = None
        self.LoadData()

        # get mouse positions
        self.mouse = False
        self.mouseDirection = pygame.Vector2(0, 0)
        self.px = 0 # player position / starting pos
        self.py = 0 # player position / starting pos

        self.sfx = None
        self.sfx_timer = 0

        self.noneAnimated = None
        self.Animated = []
        if self.animated:
            self.frame = 0
            self.loadAnimated()
            self.rect = pygame.Rect(self.player.rect.x, self.player.rect.y, self.scale[0], self.scale[1])
            self.icon = self.Animated[0]
        else:
            self.loadNoneAnimated()
            self.rect = pygame.Rect(self.player.rect.x, self.player.rect.y, self.scale[0], self.scale[1])
            self.icon = self.noneAnimated

    def draw(self):
        if self.animated:
            if (self.frame + 1) >= 12:
                self.frame = 0
            self.screen.blit(self.Animated[self.frame//3], self.rect)
            self.frame += 1
        else:
            self.screen.blit(self.noneAnimated, self.rect)

    def Hit(self, enemies):
        for enemy in enemies:
            if enemy:
                if self.rect.colliderect(enemy.rect):
                    enemy.attacked = True
                    enemy.life -= self.damage 
                    try:
                        if enemy.speed > 2:
                            enemy.speed -= self.slow

                        # sound
                        if enemy.sfx_timer == 0:
                            enemy.sfx_timer = 20
                            enemy.sfx.play()
                    except AttributeError:
                        pass
                    return True
        return False
    
    def decreaseMana(self):
        if self.player.mana > 1:
            self.player.mana -= self.mana

    def LoadData(self):
        with open('Data/weapons.json') as file:
            self.weaponData = json.load(file)

    def loadNoneAnimated(self):
        if self._type == 'weapon':
            image = f'characters/weapons/{self.name}/weapon.png'
            image = pygame.image.load(image)
            image = pygame.transform.scale(image, (self.scale[0], self.scale[1]))
        else:
            image = f'characters/weapons/potions/{self.name}/potion.png'
            image = pygame.image.load(image)
            image = pygame.transform.scale(image, (self.scale[0], self.scale[1]))

        self.noneAnimated = image

    def loadAnimated(self):
        for i in range(4):
            image = f'characters/weapons/{self.name}/weapon_{i}.png'
            image = pygame.image.load(image)
            image = pygame.transform.scale(image, (self.scale[0], self.scale[1]))
            self.Animated.append(image)

    def Trigger(self):
        keys = pygame.key.get_just_pressed()

        if keys[pygame.K_c] and self.player.mana > 5:
            if self.sfx != None:
                if self.sfx_timer == 0:
                    self.sfx_timer = 20
                    self.sfx.play()
            self.c_triggered = True
        if self.sfx_timer > 0:
            self.sfx_timer -= 1

    def Trigger_mouse(self):
        mouse = pygame.mouse.get_pressed()
        if mouse[0] and self.player.mana > 5:
            if self.sfx != None:
                if self.sfx_timer == 0:
                    self.sfx_timer = 1
                    self.sfx.play()
            self.mouse = True
            self.triggered = True
            mx, my = pygame.mouse.get_pos()
            self.mouseDirection = pygame.Vector2(mx, my)

    def Trigger_mouse2(self):
        mouse = pygame.mouse.get_pressed()
        if mouse[2] and self.player.mana > 5:
            if self.sfx != None:
                if self.sfx_timer == 0:
                    self.sfx_timer = 1
                    self.sfx.play()
            self.mouse = True
            self.triggered = True
            mx, my = pygame.mouse.get_pos()
            self.mouseDirection = pygame.Vector2(mx, my)

    def BossTraget(self, pos):
        if self.sfx != None:
            if self.sfx_timer == 0:
                self.sfx_timer = 1
                self.sfx.play()
        self.triggered = True
        mx, my = pos[0], pos[1]
        self.mouseDirection = pygame.Vector2(mx, my)

    def Throw(self, range):
        try:
            if self.duration >= range:
                self.vectorPos = pygame.Vector2(self.px, self.py)
                self.direction = (self.mouseDirection - self.vectorPos).normalize()
            self.vectorPos += self.direction * self.speed
            self.rect.x = self.vectorPos.x
            self.rect.y = self.vectorPos.y
        except ValueError:
            pass

    # add level to a weapon
    def addLevel(self):
        self.level += 1

    def Master(self, newPlayer):
        # check if the new player reach the score
        if newPlayer.score >= self.score:
            self.player = newPlayer

    def move_x(self, direction):
        self.rect.x += direction

    def move_y(self, direction):
        self.rect.y += direction

class Shield(Weapon): # regular shield

    def __init__(self, player, screen, scale, name='shield', animated=True, _type='shield'):
        super().__init__(player, screen, scale, name, animated, _type)
        self.duration = self.weaponData[self.name]['duration']
        self.absorb = self.weaponData[self.name]['absorb']
        self.mana = self.weaponData[self.name]['mana']

        self.tempShield = 0

        self.sfx = pygame.mixer.Sound('audio/shield_2.mp3')

    def weapon(self, enemies=[]):
        if self.c_triggered and self.duration > 0:
            self.rect.x = self.player.rect.x + (self.player.width - self.rect.width) / 2
            self.rect.y = self.player.rect.y + (self.player.height - self.rect.height) / 2
            self.draw()
            self.Use()
            self.decreaseMana()
            if self.player.shieldPower <= 0:
                self.c_triggered = False
            self.duration -= 1
        else:
            self.c_triggered = False
            if self.duration <= 0:
                self.Remove()
            self.duration = self.weaponData[self.name]['duration']

    def Use(self):
        if self.duration >= self.weaponData[self.name]['duration']:
            self.tempShield = self.player.shieldPower
            self.player.shieldPower += self.absorb

    def Remove(self):
        self.player.shieldPower = self.tempShield
        self.tempShield = 0

class Bomb(Weapon):

    def __init__(self, player, screen, scale, name='bomb', animated=True):
        super().__init__(player, screen, scale, name, animated)
        self.duration = 90
        self.far = 3
        self.speed = 15
        self.bframe = 0
        self.mana = 0.5
        self.damage = 0.5

        self.effectSfx = pygame.mixer.Sound('audio/bomb.mp3')
        pygame.mixer.fadeout(10)

    def weapon(self, enemies=[]):
        if self.triggered and self.duration:
            if self.duration <= 20:
                if self.sfx_timer == 0:
                    self.sfx_timer = 10
                    self.effectSfx.play()
                self.effect = True
            else:
                self.Throw()
                self.draw()
            self.decreaseMana()
            self.duration -= 1
            self.far -= 1
        else:
            # reset
            self.triggered = False
            self.duration = 90
            self.far = 3
            self.mouse = False
            self.bframe = 0
            self.effect = False
            self.px = self.player.rect.x
            self.py = self.player.rect.y

            if self.sfx_timer > 0:
                    self.sfx_timer -= 1

    def Throw(self):
        if self.far >= 3:
            self.vecPos = pygame.Vector2(self.px, self.py)
            self.direction = (self.mouseDirection - self.vecPos).normalize()
        if self.far > 0:
            self.vecPos += self.direction * self.speed
            self.rect.x, self.rect.y = self.vecPos.x, self.vecPos.y
        

class Boomerang(Weapon):

    def __init__(self, player, screen, scale, name='boomerang', animated=True):
        super().__init__(player, screen, scale, name, animated)
        self.range = 30
        self.duration = self.range
        self.speed = 20
        self.damage = 1
        self.mana = 0.6

        self.sfx = pygame.mixer.Sound('audio/boomerang.mp3')

    def weapon(self, enemies=[]):
        if self.triggered and self.mouse:
            if self.duration >= 10:
                self.Throw(self.range)
            else:
                if self.follow():
                    self.triggered = False
                    self.sfx.stop() # stop sfx
            self.draw()
            self.Hit(enemies)
            self.decreaseMana()
            self.duration -= 1
        else:
            self.triggered = False
            self.mouse = False
            self.duration = self.range
            self.px = self.player.rect.x
            self.py = self.player.rect.y
            
            if self.sfx_timer > 0:
                self.sfx_timer -= 1

    def follow(self):
        try:
            playerPos = pygame.Vector2(self.player.rect.x, self.player.rect.y)
            boomerangPos = pygame.Vector2(self.rect.x, self.rect.y)
            direction = (playerPos - boomerangPos).normalize()
            boomerangPos += direction * self.speed
            self.rect.x = boomerangPos.x
            self.rect.y = boomerangPos.y

            # check for collision
            if self.rect.colliderect(self.player.rect):
                return True
        except ValueError:
            print('Cant normalized vector of length Zero')
        
        return False

class SnowBall(Weapon):

    def __init__(self, player, screen, scale, name='snowball', animated=False):
        super().__init__(player, screen=screen, scale=scale, name=name, animated=animated)
        self.range = 10
        self.duration = self.range
        self.speed = 20
        self.damage = 0.8
        self.mana = 0.09
        self.slow = 0.5

        self.sfx = pygame.mixer.Sound('audio/snowball.mp3')

    def weapon(self, enemies=[]):
        if self.triggered and self.duration:
            self.Throw(self.range)
            self.draw()
            if self.Hit(enemies):
                self.additionalHit(enemies)
                self.triggered = False
            self.decreaseMana()
            self.duration -= 1
        else:
            # reset
            self.triggered = False
            self.mouse = False
            self.duration = self.range

            self.px = self.player.rect.x
            self.py = self.player.rect.y

            if self.sfx_timer > 0:
                self.sfx_timer -= 1

    def additionalHit(self, enemies):
        for enemy in enemies:
            if self.rect.colliderect(enemy.rect):
                if enemy.damage > 0 and enemy.name in ['zombies', 'robot', 'slime']:
                    enemy.damage -= 0.05

class DarkBall(Weapon):

    def __init__(self, player, screen, scale, name='shuriken', animated=False):
        super().__init__(player, screen, scale, name, animated)
        self.range = 80
        self.duration = self.range
        self.remove = False
        self.dir = random.choice(range(-10, 10))
        self.speed = random.choice(range(5, 10))
        self.damage = 1.5

    def weapon(self, enemies):
        if self.duration > 0:
            self.rect.y += self.speed
            self.rect.x += self.dir
            self.draw()
            self.Hit(enemies)
            self.duration -= 1
        else:
            self.duration = self.range
            self.remove = True

class Trident(Weapon):

    def __init__(self, player, screen, scale, name='trident', animated=False):
        super().__init__(player, screen, scale, name, animated)
        self.range = 20
        self.duration = self.range
        self.damage = 10
        self.speed = 23
        self.mana = 1.2
        self.px, self.py = self.rect.x, self.rect.y

        self.sfx = pygame.mixer.Sound('audio/trident.mp3')

    def weapon(self, enemies):
        if self.triggered and self.duration > 0:
            self.Throw(self.range)
            self.Rotate()
            self.Hit(enemies)
            self.decreaseMana()
            self.duration -= 1
        else:
            #reset
            self.triggered = False
            self.mouse = False
            self.duration = self.range

            self.px = self.player.rect.x
            self.py = self.player.rect.y

            if self.sfx_timer > 0:
                self.sfx_timer -= 1

    def Rotate(self):
        dis_x, dis_y = self.mouseDirection.x - self.px, self.mouseDirection.y - self.py
        angle = math.atan2(-dis_y, dis_x)
        trident = pygame.transform.rotate(self.noneAnimated, math.degrees(angle) - 90)
        trident_rect = trident.get_rect(center=(self.rect.x, self.rect.y))
        self.screen.blit(trident, trident_rect)

class Shuriken(Weapon):

    def __init__(self, player, screen, scale, name='shuriken', animated=False):
        super().__init__(player, screen, scale, name, animated)
        self.range = 20
        self.duration = self.range
        self.damage = 0.9
        self.speed = 20
        self.mana = 2

        self.px, self.py = 0,0

        self.sfx = pygame.mixer.Sound('audio/shuriken.mp3')

    def weapon(self, enemies):
        if self.triggered and self.duration > 0:
            self.Throw(self.range)
            self.rotate()
            if self.Hit(enemies):
                self.decreaseMana()
                self.triggered = False
            self.duration -= 1
        else:
            self.triggered = False
            self.mouse = False
            self.duration = self.range

            self.px = self.player.rect.x
            self.py = self.player.rect.y

            if self.sfx_timer > 0: self.sfx_timer -= 1

    def rotate(self):
        dis_x, dis_y = self.mouseDirection.x - self.px, self.mouseDirection.y - self.py
        angle = math.atan2(-dis_y, dis_x)
        trident = pygame.transform.rotate(self.noneAnimated, math.degrees(angle) - 90)
        trident_rect = trident.get_rect(center=(self.rect.x, self.rect.y))
        self.screen.blit(trident, trident_rect)

class Mjolnir(Weapon):

    def __init__(self, player, screen, scale, name='mjolnir', animated=True):
        super().__init__(player, screen, scale, name, animated)
        self.range = 30
        self.duration = self.range
        self.damage = 2.5
        self.speed = 25
        self.return_radius = 150
        self.angle = 0
        self.mana = 1

        self.sfx = pygame.mixer.Sound('audio/throw_mjolnir.mp3')

    def weapon(self, enemies):
        if self.triggered:
            if self.duration >= self.range / 2:
                self.Throw(self.range)
            else:
                if self.follow():
                    self.duration = 0
                    self.triggered = False
            self.decreaseMana()
            self.draw()
            self.Hit(enemies)
            self.duration -= 1
        else:
            #reset
            self.triggered = False
            self.mouse = False
            self.duration = self.range
            self.angle = 0

            self.px = self.player.rect.x
            self.py = self.player.rect.y

            if self.sfx_timer > 0:
                self.sfx_timer -= 1

    def follow(self):
        try:
            playerPos = pygame.Vector2(self.player.rect.x, self.player.rect.y)
            boomerangPos = pygame.Vector2(self.rect.x, self.rect.y)
            direction = (playerPos - boomerangPos).normalize()
            boomerangPos += direction * self.speed
            self.rect.x = boomerangPos.x
            self.rect.y = boomerangPos.y

            # check for collision
            if self.rect.colliderect(self.player.rect):
                return True
        except ValueError:
            print('cant normalized length of Zero')
        
        return False


class Potions(Weapon):

    def __init__(self, player, screen, scale, name, animated=False, _type='potion'):
        super().__init__(player, screen, scale, name, animated, _type)
        self.applyMana = False

        self.damage = self.weaponData[self.name]['damage']
        self.absorb = self.weaponData[self.name]['shield-support']

    def Use(self):
        if self.player.life < self.player.defaultLife:
            self.player.life += self.weaponData[self.name]['support']
        if self.player.mana < self.player.defaultMana:
            self.player.mana += self.weaponData[self.name]['plus-mana']

    def Apply(self, weapon):
        weapon.damage += self.weaponData[self.name]['damage']
        self.player.speed += self.weaponData[self.name]['speed'] # add to your speed
        if weapon.mana > self.weaponData[self.name]['weapon-support']:
            self.applyMana = True
            weapon.mana = round(weapon.mana - self.weaponData[self.name]['weapon-support'], 2)
        else:
            self.applyMana = False

        print(f'name: {weapon.name} damage: {weapon.damage} - mana: {weapon.mana}') # for testing and debuging

    def Remove(self, weapon):
        weapon.damage = round(weapon.damage - self.weaponData[self.name]['damage'], 2)
        self.player.speed -= self.weaponData[self.name]['speed']
        if self.applyMana:
            weapon.mana += self.weaponData[self.name]['weapon-support'] # theres something wrong with this

class Items(Weapon):

    def __init__(self, player, screen, scale, name, animated=False, _type='item'):
        super().__init__(player, screen, scale, name, animated, _type)
        self.code = None
        self.openData()
        self.x = None
        self.y = None

    def loadNoneAnimated(self):
        image = f'characters/icons/{self.name}.png'
        image = pygame.image.load(image)
        image = pygame.transform.scale(image, (self.scale[0], self.scale[1]))
        self.noneAnimated = image

    def openData(self):
        with open('data/items.json') as file:
            self.data = json.load(file)

    def checkItem(self):
        try:
            self.code = self.data['Items'][self.name]['code']
            self.name = self.data['Items'][self.name]['name']
            return True
        except KeyError:
            return False
        
    def move_x(self, direction):
        self.x += direction
    
    def move_y(self, direction):
        self.y += direction

class Effects(pygame.sprite.Sprite):

    def __init__(self, screen):
        self.screen = screen
        self.weapons = None # the weapons

        self.loaded = []
        self.rect = None

    def effects(self):
        if self.weapons.effect: # if the weapon effects is true or the weapon is ready to effects
            self.draw() # draw the effect animation

    def draw(self):
        # draw the animation
        if (self.weapons.bframe + 1) >= 20:
            self.weapons.bframe = 0

        self.screen.blit(self.loaded[self.weapons.bframe], (self.weapons.rect.x + ((self.weapons.rect.width - 100) / 2), self.weapons.rect.y + (self.weapons.rect.height - 100) / 2, self.loaded[0].get_width(), self.loaded[0].get_height()))
        self.weapons.bframe+=1

        # pygame.draw.rect(self.screen, (255,0,0), (self.weapons.rect.x + ((self.weapons.rect.width - 100) / 2), self.weapons.rect.y + (self.weapons.rect.height - 100) / 2, self.loaded[0].get_width(), self.loaded[0].get_height()), 2)

    def Hit(self, objs):
        if self.weapons.effect:
            for chars in objs:
                if chars.rect.colliderect((self.weapons.rect.x + ((self.weapons.rect.width - 100) / 2), self.weapons.rect.y + (self.weapons.rect.height - 100) / 2, self.loaded[0].get_width(), self.loaded[0].get_height())):
                    if chars.name in ['zombies']:
                        chars.attacked = True
                    chars.life -= self.weapons.damage

    def loadEffects(self, equiped):
        self.weapons = equiped

        try: # check if the weapon has effects
            for i in range(20):
                image = f'characters/effects/{self.weapons.name}/second/{i}.png'
                image = pygame.image.load(image)
                image = pygame.transform.scale(image, (100, 100))
                self.loaded.append(image)

            self.rect = pygame.Rect(self.weapons.rect.x + ((self.weapons.rect.width - 100) / 2), self.weapons.rect.y + (self.weapons.rect.height - 100) / 2, self.loaded[0].get_width(), self.loaded[0].get_height())

        except FileNotFoundError:
            pass