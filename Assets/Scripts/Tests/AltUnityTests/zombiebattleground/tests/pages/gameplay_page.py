from .base import CZBTests
import time
from enum import Enum
from random import randint

OPPONENT_HEALTH = 'Opponent/OverlordArea/RegularModel/RegularPosition/Avatar/Deffence/DefenceText'
PLAYER_HEALTH = 'Player/OverlordArea/RegularModel/RegularPosition/Avatar/Deffence/DefenceText'


class BoardCard(object):
    def __init__(self, altunityobject, altdriver):
        self.altdriver = altdriver
        self.driver = altunityobject.appium_driver
        self.alt_object = altunityobject
        self.name = ""
        self.attack = 0
        self.defense = 0
        self.type = []
        self.frozen = self.is_frozen(self.alt_object)
        self.get_card_information()


    def get_card_information(self):
        self.alt_object.mobile_tap()
        time.sleep(2)
        creature_cards = self.altdriver.find_elements('CreatureCard(Clone)')
        card_object = creature_cards[-1]
        self.name = self.get_card_name(card_object)
        self.attack = int(self.get_card_attack(card_object))
        self.defense = int(self.get_card_defense(card_object))
        self.type = self.get_card_type(card_object)
        self.driver.tap([[float(100), float(100)]],1000)
        time.sleep(2)

    def get_card_name(self, card_object):
        name_object = self.altdriver.find_element('id(' + str(card_object.id) + ')/TitleText')
        return name_object.get_component_property('TMPro.TextMeshPro', 'text', 'Unity.TextMeshPro')

    def get_card_attack(self, card_object):
        attack_object = self.altdriver.find_element('id(' + str(card_object.id) + ')/AttackText')
        return attack_object.get_component_property('TMPro.TextMeshPro', 'text', 'Unity.TextMeshPro')

    def get_card_defense(self, card_object):
        defense_object = self.altdriver.find_element('id(' + str(card_object.id) + ')/DeffensText')
        return defense_object.get_component_property('TMPro.TextMeshPro', 'text', 'Unity.TextMeshPro')

    def get_card_type(self, card_object):
        type = []
        type_objects = self.altdriver.find_elements(
            'id(' + str(card_object.id) + ')/Group_LeftBlockInfo/Tooltip_BuffOnCardInfo(Clone)/Text_CallType')
        for type_object in type_objects:
            type.append(type_object.get_component_property('TMPro.TextMeshPro', 'text', 'Unity.TextMeshPro'))
        return type

    def is_frozen(self, board_creature):
        frozen_component = self.altdriver.find_element('id(' + str(board_creature.id) + ')/Other/Frozen')
        frozen_component_color = frozen_component.get_component_property("UnityEngine.SpriteRenderer", "color",
                                                                "UnityEngine.CoreModule")
        color_alpha = frozen_component_color.split(',')[3].split(')')[0]
        if color_alpha == "0.000":
            return False
        return True


class PlayerCard(BoardCard):
    def __init__(self, altunityobject, altdriver):
        super(PlayerCard,self).__init__(altunityobject, altdriver)
        self.active = self.is_active(self.alt_object)

    def is_active(self, board_creature):
        #TODO change to search for animated green frame
        sleeping_particles = self.altdriver.find_element('id(' + str(board_creature.id) + ')/Other/SleepingParticles',
                                                         enabled=False)
        return not sleeping_particles.enabled =='True'


class OpponentCard(BoardCard):
    def __init__(self, altunityobject, altdriver):
        super(OpponentCard,self).__init__(altunityobject, altdriver)
        self.score = -1


class ExtraMoves(Enum):
    NORMAL = 0
    FROZEN = 1
    HEAVY = 2
    ENEMYZOMBIE = 3
    ALLYZOMBIE = 4
    ZOMBIE = 5
    ENEMY = 6
    ENEMYWATERPLAY = 7
    ALL = 8


class Type(Enum):
    DAMAGE = 0
    HEAL = 1
    DESTROY = 2
    HEALORDAMAGE = 3
    OTHER = 4


list_text_entry = [
    ('<b>Entry:</b> 2 damage to an enemy', ExtraMoves.ENEMYZOMBIE, Type.DAMAGE, 2),
    ('<b>Entry:</b> Distract an enemy zombie', ExtraMoves.ENEMYZOMBIE, Type.OTHER, 0),
    ('<b>Entry:</b> 1 damage to a zombie', ExtraMoves.ENEMYZOMBIE, Type.DAMAGE, 1),
    ('<b>Entry:</b> Destroy a Frozen zombie', ExtraMoves.FROZEN, Type.DAMAGE, 0),
    ('<b>Entry:</b> Freeze zombie and adjacent zombies, 2 damage if Frozen', ExtraMoves.ZOMBIE, Type.DAMAGE, 0),
    ('<b>Entry:</b> 2 damage to an enemy if a Water zombie in play', ExtraMoves.ENEMYWATERPLAY, Type.DAMAGE, 2),
    ('<b>Entry:</b> Return a zombie to its owner\'s Hand', ExtraMoves.ZOMBIE, Type.OTHER, 0),
    ('<b>Entry:</b> Return an ally zombie to your Hand', ExtraMoves.ALLYZOMBIE, Type.OTHER, 0),
    ('<b>Entry:</b> 1 damage to a zombie, Swing 1', ExtraMoves.ZOMBIE, Type.DAMAGE, 1),
    ('Restore 4 Def to a zombie', ExtraMoves.ZOMBIE, Type.HEAL, 4),
    ('2 damage to a zombie, and adjacent zombies', ExtraMoves.ZOMBIE, Type.DAMAGE, 2),
    ('3 damage to a enemy', ExtraMoves.ENEMY, Type.DAMAGE, 3),
    ('3 damage to an enemy OR restore 3 Def to an ally', ExtraMoves.ALL, Type.HEALORDAMAGE, 3),
    ('2 damage to a zombie and Distract it', ExtraMoves.ZOMBIE, Type.DAMAGE, 2),
    ('4 damage to a zombie, and adjacent zombies', ExtraMoves.ZOMBIE, Type.DAMAGE, 4),
    ('+4 Atk to a zombie; End: 4 damage to this zombie', ExtraMoves.ZOMBIE, Type.DAMAGE, 4),
    ('5 damage to a enemy', ExtraMoves.ZOMBIE, Type.DAMAGE, 5),
    ('Give an ally Zombie +3/+3; <b>End:</b> -3 DEF', ExtraMoves.ALLYZOMBIE, Type.HEAL, 3)

]


class PlayerHand:
    def __init__(self, alt_object, altdriver):
        self.alt_object = alt_object
        self.altdriver = altdriver
        self.extraMoves = ExtraMoves.NORMAL
        self.playable = self.isPlayable(self.alt_object)
        self.type = Type.OTHER
        self.value = 0
        self.get_card_effect()

    def get_card_effect(self):
        # TODO change name
        text = self.altdriver.find_element('id(' + str(self.alt_object.id) + ')/BodyText').get_component_property(
            'TMPro.TextMeshPro', 'text',
            'Unity.TextMeshPro')
        for entry in list_text_entry:
            if text in entry[0]:
                self.extraMoves = entry[1]
                self.type = entry[2]
                self.value = entry[3]
                break

    def isPlayable(self, card):
        glow = self.altdriver.find_element('id(' + str(card.id) + ')/GlowContainer/Glow', enabled=False)
        return glow.enabled == 'True'

    def swipe_object_position(self):
        return self.altdriver.find_element('id(' + str(self.alt_object.id) + ')/GooText')


class Gameplay_Page(CZBTests):
    def __init__(self, altdriver, driver):
        self.altdriver = altdriver
        self.driver = driver
        self.altdriver.wait_for_current_scene_to_be('GAMEPLAY')
        self.end_turn_button = self.altdriver.wait_for_element('EndTurnButton/_1_btn_endturn')
        self.player_board = self.altdriver.wait_for_element('Battlefield/PlayerBoard')
        self.opponent_board = self.altdriver.wait_for_element('Battlefield/OpponentBoard')
        self.opponent_face = self.altdriver.wait_for_element(
            'Opponent/OverlordArea/RegularModel/RegularPosition/Avatar/OverlordImage')
        self.player_primary_spell = self.altdriver.wait_for_element('Player/Object_SpellPrimary')
        self.player_secondary_spell = self.altdriver.wait_for_element('Player/Object_SpellSecondary')

    def get_cards_that_are_in_hand(self):
        cards = self.altdriver.find_elements('CreatureCard(Clone)')
        cards.extend(self.altdriver.find_elements('ItemCard(Clone)'))
        cards.sort(key=lambda element: float(element.x))
        return cards

    def swipe_card_from_hand_to_board(self, card):
        cardGoo = self.altdriver.wait_for_element(
            'id(' + str(card.id) + ')/GooText')
        self.driver.swipe(int(cardGoo.x), int(cardGoo.mobileY),
                          int(self.player_board.x), int(self.player_board.mobileY), 2000)
        time.sleep(4)

    def get_player_health(self):
        return int(self.altdriver.find_element(PLAYER_HEALTH).get_component_property('TMPro.TextMeshPro', 'text',
                                                                                     'Unity.TextMeshPro'))

    def get_opponent_health(self):
        return int(self.altdriver.find_element(OPPONENT_HEALTH).get_component_property('TMPro.TextMeshPro', 'text',
                                                                                       'Unity.TextMeshPro'))

    def get_player_hand(self):
        player_hand_cards = []
        hand_creatures = self.altdriver.find_elements('CreatureCard(Clone)')
        for hand_creature in hand_creatures:
            player_hand_cards.append(PlayerHand(hand_creature, self.altdriver))
        hand_items = self.altdriver.find_elements('ItemCard(Clone)')
        for hand_item in hand_items:
            player_hand_cards.append(PlayerHand(hand_item, self.altdriver))
        return player_hand_cards

    def get_playable_cards(self):
        playable_cards = []
        hand_cards = self.get_player_hand()
        for card in hand_cards:
            if card.playable:
                playable_cards.append(card)
        return playable_cards

    def play_card(self):
        while True:
            playable_cards = self.get_playable_cards()
            if len(playable_cards) != 0:
                random_number = randint(0, len(playable_cards) - 1)
                player_card = playable_cards[random_number]
                self.swipe_card_from_hand_to_board(player_card.alt_object)
                if player_card.extraMoves != ExtraMoves.NORMAL:
                    if player_card.extraMoves == ExtraMoves.FROZEN:
                        frozen_zombies = self.get_all_frozen_zombies()
                        if len(frozen_zombies) != 0:
                            frozen_zombies[0].alt_object.mobile_tap()
                    elif player_card.extraMoves == ExtraMoves.HEAVY:
                        heavy_zombies = self.get_all_zombies_on_board()
                        if len(heavy_zombies) != 0:
                            heavy_zombies[0].alt_object.mobile_tap()
                    elif player_card.extraMoves == ExtraMoves.ENEMYZOMBIE:
                        enemy_zombies = self.get_all_enemy_board_creatures()
                        if len(enemy_zombies) != 0:
                            enemy_zombies[0].alt_object.mobile_tap()
                    elif player_card.extraMoves == ExtraMoves.ENEMY or player_card.extraMoves == ExtraMoves.ALL:
                        enemy_zombies = self.get_all_enemy_board_creatures()
                        if len(enemy_zombies) != 0:
                            enemy_zombies[0].alt_object.mobile_tap()
                        else:
                            self.opponent_face.mobile_tap()
                    elif player_card.extraMoves == ExtraMoves.ALLYZOMBIE:
                        ally_zombies = self.get_player_board_creatures()
                        if len(ally_zombies) != 0:
                            ally_zombies[0].alt_object.mobile_tap()
                    elif player_card.extraMoves == ExtraMoves.ENEMYWATERPLAY:
                        enemy_zombies = self.get_all_enemy_board_creatures()
                        if len(enemy_zombies) != 0:
                            enemy_zombies[0].alt_object.mobile_tap()
                        else:
                            self.opponent_face.mobile_tap()
                    elif player_card.extraMoves == ExtraMoves.ZOMBIE:
                        zombies=self.get_all_zombies_on_board()
                        zombies[0].alt_object.mobile_tap()
            else:
                break

    def get_player_board_creatures(self):
        cards = self.altdriver.find_elements('PlayerBoard/BoardCreature(Clone)')
        cards.sort(key=lambda element: float(element.x))
        return cards

    def get_opponent_board_creatures(self):
        cards = self.altdriver.find_elements('OpponentBoard/BoardCreature(Clone)')
        cards.sort(key=lambda element: float(element.x))
        return cards

    def end_turn_and_wait_for_your_turn(self):
        self.end_turn_button.mobile_tap()
        time.sleep(4)
        self.altdriver.wait_for_element('YourTurnPopup(Clone)')
        self.altdriver.wait_for_element('id(' + str(self.end_turn_button.id) + ')/EndTurnGlowEffect', timeout=60)
        time.sleep(5)  # sleep is to wait for the card player draws go to player hand

    def swipe_board_card_to_opponent_face(self, cardPosition):
        player_board_creature = self.get_player_board_creatures()
        player_board_creature[cardPosition].mobile_dragToElement(self.opponent_face, 2)
        time.sleep(4)

    def swipe_primary_spell_to_opponent_face(self):
        self.player_primary_spell.mobile_dragToElement(self.opponent_face, 2)
        time.sleep(4)

    def swipe_primary_spell_to_opponent_creature(self, card):
        enemy_board_creature = self.get_opponent_board_creatures()
        self.player_primary_spell.mobile_dragToElement(card, 2)
        time.sleep(4)

    def swipe_board_card_to_opponent_creature(self, player_card, opponent_card):
        player_board_creature = self.get_player_board_creatures()
        enemy_board_creature = self.get_opponent_board_creatures()
        player_card.mobile_dragToElement(opponent_card,2)
        time.sleep(6)

    def play_player_turn(self):

        ##play buff cards/ utility
        self.action_phase()
        self.play_card()
        self.end_turn_and_wait_for_your_turn()

    def action_phase(self):

        damaging_cards = self.get_every_possible_thing_that_can_cause_damage_this_turn()
        if len(damaging_cards) != 0:
            while self.attack_heavy_zombies():
                pass

            damaging_cards = self.get_every_possible_thing_that_can_cause_damage_this_turn()
            if len(damaging_cards) != 0:
                if self.is_lethal_available(damaging_cards) or len(self.get_opponent_board_creatures()) == 0:
                    for damaging_card in damaging_cards:
                        self.attack_opponent_face(damaging_card)
                else:
                    while self.trade_cards():
                        pass

    def calculate_card_score(self, player_card_attack, player_card_health, opponent_card_attack, opponent_card_health):
        if player_card_attack == opponent_card_health and player_card_health > opponent_card_attack:
            score = 100 - (player_card_health - opponent_card_attack)
            if score < 90:
                score = 90
            return score
        if player_card_attack > opponent_card_health and player_card_health > opponent_card_attack:
            score = 90 - (player_card_attack - opponent_card_health) - (player_card_health - opponent_card_attack)
            if score < 70:
                score = 70
            return score
        if player_card_attack >= opponent_card_health and player_card_health <= opponent_card_attack:
            score = 69 + (opponent_card_health - player_card_attack) + (player_card_health - opponent_card_attack)
            if score < 60:
                score = 60
            return score
        if player_card_attack < opponent_card_health and player_card_health > opponent_card_attack:
            score = 59 - (opponent_card_health - player_card_attack) - (player_card_health - opponent_card_attack)
            if score < 30:
                score = 30
            return score
        if player_card_attack < opponent_card_attack and player_card_health <= opponent_card_attack:
            score = 29 - (opponent_card_health - player_card_attack) - (opponent_card_attack - player_card_health)
            if (score < 0):
                score = 0
            return score
        print("Error: situation not covered ", player_card_attack, player_card_health, opponent_card_attack,
              opponent_card_health)
        return -1

    def get_all_enemy_board_creatures(self):
        enemy_board_creatures = []
        board_creatures_objects = self.altdriver.find_elements('Battlefield/OpponentBoard/BoardCreature(Clone)')
        for opponent_card in board_creatures_objects:
            board_creature = OpponentCard(opponent_card, self.altdriver)
            enemy_board_creatures.append(board_creature)
        return enemy_board_creatures

    def get_all_player_board_creature(self):
        player_board_creatures = []
        board_creatures_objects = self.altdriver.find_elements('Battlefield/PlayerBoard/BoardCreature(Clone)')
        for player_card in board_creatures_objects:
            board_creature = PlayerCard(player_card, self.altdriver)
            player_board_creatures.append(board_creature)
        return player_board_creatures

    def get_every_possible_thing_that_can_cause_damage_this_turn(self):
        ##Create list of tuple(altunityobject,type,name,attack,health,attacked)
        cards_that_can_do_damage = self.get_all_active_zombies_from_board()
        ##Add all zombies,spell than can do damage
        return cards_that_can_do_damage

    def get_all_active_zombies_from_board(self):
        active_zombies = []
        list_player_zombies_on_board = self.get_all_player_board_creature()
        for card in list_player_zombies_on_board:
            if card.active:
                active_zombies.append(card)
        return active_zombies

    # def get_all_damaging_cards_from_hand(self):
    #     ##return every card from hand that could do damage


    def get_all_opponent_heavy_cards(self):
        heavy_opponents = []
        for opponent_card in self.get_all_enemy_board_creatures():
            if 'HEAVY' in opponent_card.type:
                heavy_opponents.append(opponent_card)

        return heavy_opponents

    def is_lethal_available(self, available_damage):
        total_damage_available = 0
        for element in available_damage:
            total_damage_available = total_damage_available + int(element.attack)
        opponent_health = self.get_opponent_health()
        return total_damage_available > opponent_health

    def is_card_from_hand_able_to_damage_and_playable_this_turn(self, card):
        if self.altdriver.find_element('id(' + card.id + ')/GlowContainer/Glow', enabled=False).enabled == 'true':
            body_text_object = self.altdriver.find_element('id(' + card.id + ')/BodyText')
            body_text = self.read_tmp_GUI_text(body_text_object)
            print("card text is: " + body_text)
            if "<b>Feral:</b>" in body_text:
                return True

                ##TODO add more different type of card that can damage

        return False

    def attack_heavy_zombies(self):
        cards_able_to_do_damage = self.get_every_possible_thing_that_can_cause_damage_this_turn()
        taunts = self.get_all_opponent_heavy_cards()

        if len(cards_able_to_do_damage) == 0 or len(taunts) == 0:
            return False
        for damage_card in cards_able_to_do_damage:
            for taunt_card in taunts:
                score = self.calculate_card_score(damage_card.attack, damage_card.defense, taunt_card.attack,
                                                  taunt_card.defense)
                if score < taunt_card.score:
                    taunt_card[4] = score

        for taunt_card in taunts:
            for damage_card in cards_able_to_do_damage:
                score = self.calculate_card_score(damage_card.attack, damage_card.defense, taunt_card.attack,
                                                  taunt_card.defense)
                if score == taunt_card.score:
                    self.swipe_board_card_to_opponent_creature(damage_card, taunt_card)
                    return True
        print("Error no how I got here")

    def trade_cards(self, score_limit=50):

        cards_able_to_do_damage = self.get_every_possible_thing_that_can_cause_damage_this_turn()
        opponent_creatures = self.get_all_enemy_board_creatures()

        if len(cards_able_to_do_damage) == 0:
            return False
        if len(opponent_creatures) == 0:
            for damaging_card in cards_able_to_do_damage:
                self.attack_opponent_face(damaging_card)
            return False

        for damage_card in cards_able_to_do_damage:
            for opponent_creature in opponent_creatures:
                score = self.calculate_card_score(damage_card.attack, damage_card.defense, opponent_creature.attack,
                                                  opponent_creature.defense)
                if score < opponent_creature.score:
                    opponent_creature.score = score
        opponent_creatures = sorted(opponent_creatures, key=lambda creature: creature.score)
        print("Score = ",opponent_creatures[0].score)
        if opponent_creatures[0].score > score_limit:
            for opponent_creature in opponent_creatures:
                for damage_card in cards_able_to_do_damage:
                    score = self.calculate_card_score(damage_card.attack, damage_card.defense, opponent_creature.attack,
                                                      opponent_creature.defense)
                    if score == opponent_creature.score:
                        self.swipe_board_card_to_opponent_creature(damage_card, opponent_creature)
                    return True
        else:
            for damage_card in cards_able_to_do_damage:
                self.attack_opponent_face(damage_card.alt_object)
            return False

    def mulligan_high_cost_cards(self):
        self.altdriver.wait_for_element('MulliganPopup(Clone)')
        mulligan_cards = self.altdriver.find_elements('MulliganCard_Unit(Clone)')
        replace_panel = self.altdriver.find_element('Replace_Panel')
        button_keep = self.altdriver.find_element('Button_Keep')
        for mulligan_card in mulligan_cards:
            card_cost_object = self.altdriver.find_element('id(' + mulligan_card.id + ')/Text_Goo')
            card_cost = self.read_tmp_UGUI_text(card_cost_object)
            if (int(card_cost) > 3):
                mulligan_card.mobile_dragToElement(replace_panel, 2)
        button_keep.mobile_tap()

    def attack_opponent_face(self, damage_card):
        # TODO add for spell and feral creature from hand
        self.swipe_board_card_to_opponent_face(damage_card)

    def get_all_zombies_on_board(self):
        zombies = self.get_all_enemy_board_creatures()
        zombies = zombies + self.get_all_player_board_creature()
        return zombies

    def get_all_frozen_zombies(self):
        frozen_zombies = []
        all_zombies = self.get_all_zombies_on_board()
        for zombie in all_zombies:
            if zombie.frozen:
                frozen_zombies.append(zombie)
        return frozen_zombies

    def get_all_heavy_zombies(self):
        heavy_zombies = []
        all_zombies = self.get_all_zombies_on_board()
        for zombie in all_zombies:
            if 'HEAVY' in zombie.type:
                heavy_zombies.append(zombie)
        return heavy_zombies

    def play_a_match(self):
        self.mulligan_high_cost_cards()
        while True:
            self.play_player_turn()
